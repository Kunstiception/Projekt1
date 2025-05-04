using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : Manager
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform[] _firstWaypoints;
    [SerializeField] private GameObject[] _days;
    [SerializeField] private TextMeshProUGUI _daysCounter;
    [SerializeField] private TextMeshProUGUI _dayNightVisual;

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

        // Public string nie null
        if(MainManager.Instance.LastWayPoint.Length > 0)
        {
            var index = MainManager.Instance.WayPoints.IndexOf(MainManager.Instance.LastWayPoint);

            MainManager.Instance.WayPointTypes[index] = 0;

        }
        else if(PlayerManager.Instance != null)
        {
            PlayerManager.Instance.InitializePlayerStats();
        }

        if(MainManager.Instance.CurrentDay > 0)
        {
            MainManager.Instance.SaveAll();
        }

        if(MainManager.Instance.IsDay == true)
        {
            _dayNightVisual.text = "Daytime";
        }
        else
        {
            _dayNightVisual.text = "Nighttime";
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

    // Ist kein Spielstand vorhanden, werden die Wegpunkte und ihre Typen des jetzigen Tages im MainManager abgespeichert
    private void GetWayPoints()
    {
        var foundWayPoints = _days[MainManager.Instance.CurrentDay].GetComponentsInChildren<WayPoint>();

        // 0 = empty, 1 = fight, 2 = loot, 3 = interaction, 4 = resting
        foreach(var wayPoint in foundWayPoints)
        {
            MainManager.Instance.WayPoints.Add(wayPoint.gameObject.name);

            if(wayPoint.isRestingWayPoint == 0)
            {
                wayPoint.SetType(4);
                MainManager.Instance.WayPointTypes.Add(4);
                continue;
            }

            var randomIndex = Random.Range(1, 4);
            MainManager.Instance.WayPointTypes.Add(randomIndex);
            wayPoint.SetType(randomIndex);          
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

        SceneManager.LoadScene(_nextSceneIndex);
    }
}
