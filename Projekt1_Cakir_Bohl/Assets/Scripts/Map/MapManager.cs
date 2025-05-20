using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform[] _firstWaypoints;
    [SerializeField] private GameObject[] _days;
    [SerializeField] private TextMeshProUGUI _daysCounter;
    [SerializeField] private SpriteRenderer _timeOfDayIcon;
    [SerializeField] private Sprite _dayIcon;
    [SerializeField] private Sprite _nightIcon;

    private int _nextSceneIndex;
    private float _movementLength;
    private float _timer;
    private string _interactableTag = "Interactable";
    private string _nonInteractableTag = "Non_Interactable";
    private Transform _nextWaypoint;
    private Coroutine _movementCoroutine;
    private Transform _currentWaypoint;

    // Hier werden die Wegpunkte entweder erstmalig gesetzt oder die Informationen aus einem existierenden Spielstand aus dem MainManager geladen
    void Start()
    {
        if(MainManager.Instance == null)
        {
            return;
        }

        foreach(GameObject day in _days)
        {
            if(day == _days[MainManager.Instance.CurrentDay])
            {
                day.SetActive(true);
            }
            else
            {
                day.SetActive(false);
            }
        }

        if(MainManager.Instance.WayPoints.Count == 0)
        {
            GetWayPoints();
        }
        else
        {
            SetWayPointTypes();
        }

        if(MainManager.Instance.LastWayPoint.Length == 0 && PlayerManager.Instance != null && MainManager.Instance.CurrentDay == 0)
        {
            PlayerManager.Instance.InitializePlayerStats();
        }

        MainManager.Instance.SaveAll();
 
        if(MainManager.Instance.IsDay == true)
        {
            _timeOfDayIcon.sprite = _dayIcon;
        }
        else
        {
            _timeOfDayIcon.sprite = _nightIcon;
        }

        SetCurrentPosition();
        SetNextWayPointsTag();      

        _daysCounter.text = $"Day: {MainManager.Instance.CurrentDay + 1}";
    }

    private void OnEnable()
    {      
        WayPoint.clickAction += SetNextPosition;
    }

    private void OnDisable()
    {
        WayPoint.clickAction -= SetNextPosition;
    }

    // Ist kein Spielstand vorhanden, werden die Wegpunkte und ihre Type des jetzigen Tages im MainManager abgespeichert
    // Es soll immer genau 2 Interactions geben und die restlichen Wegpunkte (abzüglich Anfang und Ende) teilen sich Fight und Looting
    // Auch diesen wird aus Balancing-Gründen eine maximale Anzahl vorgegeben
    private void GetWayPoints()
    {
        var foundWayPoints = _days[MainManager.Instance.CurrentDay].GetComponentsInChildren<WayPoint>();
        List<WayPoint> tempWayPoints = foundWayPoints.ToList<WayPoint>();
        int numberOfInteractions = 0;
        int numberOfFights = 0;
        int numberOfLooting = 0;
        int randomIndex = 0;

        // 0 = empty, 1 = fight, 2 = loot, 3 = interaction, 4 = resting
        for (int i = 0; i < foundWayPoints.Length; i++)
        {
            WayPoint currentWayPoint = tempWayPoints[Random.Range(0, tempWayPoints.Count)];

            MainManager.Instance.WayPoints.Add(currentWayPoint.gameObject.name);

            if (currentWayPoint.wayPointCategory == WayPoint.WayPointCategory.IsResting)
            {
                currentWayPoint.SetType(4);
                MainManager.Instance.WayPointTypes.Add(4);
                tempWayPoints.Remove(currentWayPoint);

                continue;
            }

            if (currentWayPoint.wayPointCategory == WayPoint.WayPointCategory.IsStart)
            {
                currentWayPoint.SetType(0);
                MainManager.Instance.WayPointTypes.Add(0);
                tempWayPoints.Remove(currentWayPoint);

                continue;
            }

            if (numberOfInteractions < 2)
            {
                randomIndex = 3;
            }
            else
            {
                randomIndex = Random.Range(1, 3);
            }

            switch (randomIndex)
            {
                case 1:
                    // Anzahl der Wegpunkte ohne Anfang und Ende
                    if (numberOfFights < (foundWayPoints.Length - 2) / 2)
                    {
                        numberOfFights++;
                    }
                    else
                    {
                        randomIndex = 2;
                    }

                    break;

                case 2:
                    if (numberOfLooting < (foundWayPoints.Length - 2) / 2)
                    {
                        numberOfLooting++;
                    }
                    else
                    {
                        randomIndex = 1;
                    }

                    break;


                case 3:
                    numberOfInteractions++;

                    break;
            }

            MainManager.Instance.WayPointTypes.Add(randomIndex);
            currentWayPoint.SetType(randomIndex);
            tempWayPoints.Remove(currentWayPoint);
        }
    }

    // Vorhandenen Spielstand laden
    private void SetWayPointTypes()
    {
        var foundWayPoints = _days[MainManager.Instance.CurrentDay].GetComponentsInChildren<WayPoint>();

        foreach(var wayPoint in foundWayPoints)
        {
            //https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.list-1.indexof?view=net-8.0
            var index = MainManager.Instance.WayPoints.IndexOf(wayPoint.gameObject.name);

            wayPoint.SetType(MainManager.Instance.WayPointTypes[index]);
        }   
    }

    // Startposition setzen
    private void SetCurrentPosition()
    {
        if(MainManager.Instance == null)
        {
            return;
        }
         
        //string nie null
        if(MainManager.Instance.LastWayPoint.Length == 0)
        {
            _currentWaypoint = _firstWaypoints[MainManager.Instance.CurrentDay];
        }
        else
        {
            _currentWaypoint = GameObject.Find(MainManager.Instance.LastWayPoint).transform;
        }

        _player.position = _currentWaypoint.position;
    }

    // Tags der Wegpunkte setzen, ob diese anklickbar sind oder nicht
    private void SetNextWayPointsTag()
    {
        if(_nextWaypoint == null)
        {
            foreach(Transform wayPoint in _currentWaypoint.GetComponent<WayPoint>().AdjacentWaypoints)
            {
                wayPoint.gameObject.tag = _interactableTag;
            }

            return;
        }

        foreach(Transform wayPoint in _nextWaypoint.GetComponent<WayPoint>().AdjacentWaypoints)
        {
            wayPoint.gameObject.tag = _interactableTag;
        }
    }

    // Setzen der Position des nächsten Wegpunkts nach Klick auf diesen
    private void SetNextPosition(Transform transform)
    {
        if(_movementCoroutine != null)
        {
            return;
        }

        _nextWaypoint = transform;
        _movementCoroutine = StartCoroutine(MovementCoroutine(_player.position, _nextWaypoint.position));
    }

    // Coroutine für Bewegung des Players auf der Oberwelt zwischen zwei Wegpunkten
    private IEnumerator MovementCoroutine(Vector2 startPosition, Vector2 targetPosition)
    {
        Vector2 initialPosition = startPosition;
        Vector2 finalPosition = targetPosition;

        _movementLength = Vector2.Distance(initialPosition, finalPosition);

        float movementDuration = _movementLength / GameConfig.MovementSpeed;

        while (_timer < movementDuration)
        {
            _timer += Time.deltaTime;
            _player.position = Vector2.Lerp(initialPosition, finalPosition,
                (_timer / movementDuration));
            yield return null;
        }

        _timer = 0;
        transform.position = finalPosition;
        _movementCoroutine = null;
        _currentWaypoint = _nextWaypoint;

        switch (_nextWaypoint.GetComponent<WayPoint>().WayPointType)
        {
            // Empty = 3, Combat = 4, Loot = 5, Interaction = 6, Resting = 7
            case "Empty":
                _nextSceneIndex = 3;
                break;
            case "Fight":
                _nextSceneIndex = 4;
                break;
            case "Loot":
                _nextSceneIndex = 5;
                break;
            case "Interaction":
                _nextSceneIndex = 6;
                break;
            case "Resting":
                _nextSceneIndex = 7;
                break;
            default:
                Debug.LogError("No scene found!");
                break;
        }

        // Object id ändert sich mit jeder Session, daher name
        MainManager.Instance.LastWayPoint = _currentWaypoint.name;

        var index = MainManager.Instance.WayPoints.IndexOf(MainManager.Instance.LastWayPoint);
        MainManager.Instance.WayPointTypes[index] = 0;

        SceneManager.LoadScene(_nextSceneIndex);
    }
}
