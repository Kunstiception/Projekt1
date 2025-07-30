using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : Manager
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform[] _firstWaypoints;
    [SerializeField] private GameObject[] _days;
    [SerializeField] private WayPoint _dogFirstDay;
    //[SerializeField] private TextMeshProUGUI _daysCounter;
    [SerializeField] private SpriteRenderer _timeOfDayIcon;
    [SerializeField] private Sprite _dayIcon;
    [SerializeField] private Sprite _nightIcon;

    private int _nextSceneIndex;
    private float _movementLength;
    private float _timer;
    private string _interactableTag = "Interactable";
    private Transform _nextWaypoint;
    private Coroutine _movementCoroutine;
    private Transform _currentWaypoint;
    private Animator _playerAnimator;

    // Hier werden die Wegpunkte entweder erstmalig gesetzt oder die Informationen aus einem existierenden Spielstand aus dem MainManager geladen
    void Start()
    {
        if (MainManager.Instance == null)
        {
            throw new NullReferenceException("MainManager not set to an instance of an object.");
        }

        Cursor.visible = true;

        foreach (GameObject day in _days)
        {
            if (day == _days[MainManager.Instance.CurrentDay])
            {
                day.SetActive(true);
            }
            else
            {
                day.SetActive(false);
            }
        }

        if (MainManager.Instance.WayPoints.Count == 0)
        {
            GetWayPoints();
        }
        else
        {
            SetWayPointTypes();
        }

        if (MainManager.Instance.LastWayPoint.Length == 0 && PlayerManager.Instance != null && MainManager.Instance.CurrentDay == 0)
        {
            PlayerManager.Instance.InitializePlayerStats();
        }

        MainManager.Instance.SaveAll();

        if (MainManager.Instance.IsDay == true)
        {
            _timeOfDayIcon.sprite = _dayIcon;
        }
        else
        {
            _timeOfDayIcon.sprite = _nightIcon;
        }

        _playerAnimator = _player.GetComponentInChildren<Animator>();

        SetCurrentPosition();
        SetNextWayPointsTag();

        PlayerManager.Instance.HasFinishedDay = false;
        //_daysCounter.text = $"Day: {MainManager.Instance.CurrentDay + 1}";
    }

    public override void OnEnable()
    {
        base.OnEnable();

        WayPoint.clickAction += SetNextPosition;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        WayPoint.clickAction -= SetNextPosition;
    }

    public override void ListenForSkipOrAuto()
    {
        return;
    }

    // Ist kein Spielstand vorhanden, werden die Wegpunkte und ihre Type des jetzigen Tages im MainManager abgespeichert
    // Es soll immer genau eine Interaction geben und die restlichen Wegpunkte (abzüglich Anfang und Ende) teilen sich Fight und Looting
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
                WayPoint currentWayPoint = tempWayPoints[UnityEngine.Random.Range(0, tempWayPoints.Count)];

                MainManager.Instance.WayPoints.Add(currentWayPoint.gameObject.name);

                // Beim ersten Tag ist der Interaktions-Wegpunkt festgelegt (Hund-Tutorial)
                if (MainManager.Instance.CurrentDay == 0 && currentWayPoint == _dogFirstDay)
                {
                    currentWayPoint.SetType(3);
                    MainManager.Instance.WayPointTypes.Add(3);
                    tempWayPoints.Remove(currentWayPoint);

                    numberOfInteractions++;

                    continue;
                }

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

                if (numberOfInteractions < 1 && MainManager.Instance.CurrentDay != 0)
                {
                    randomIndex = 3;
                }
                else
                {
                    randomIndex = UnityEngine.Random.Range(1, 3);
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

        foreach (var wayPoint in foundWayPoints)
        {
            //https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.list-1.indexof?view=net-8.0
            var index = MainManager.Instance.WayPoints.IndexOf(wayPoint.gameObject.name);

            wayPoint.SetType(MainManager.Instance.WayPointTypes[index]);
        }
    }

    // Startposition setzen
    private void SetCurrentPosition()
    {
        if (MainManager.Instance == null)
        {
            return;
        }

        //string nie null
        if (MainManager.Instance.LastWayPoint.Length == 0)
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
        if (_nextWaypoint == null)
        {
            foreach (Transform wayPoint in _currentWaypoint.GetComponent<WayPoint>().AdjacentWaypoints)
            {
                if (wayPoint.GetComponent<WayPoint>().WayPointType == "Empty")
                {
                    continue;
                }

                wayPoint.gameObject.tag = _interactableTag;
            }

            return;
        }

        foreach (Transform wayPoint in _nextWaypoint.GetComponent<WayPoint>().AdjacentWaypoints)
        {
            if (wayPoint.GetComponent<WayPoint>().WayPointType == "Empty")
            {
                continue;
            }

            wayPoint.gameObject.tag = _interactableTag;
        }
    }

    // Setzen der Position des nächsten Wegpunkts nach Klick auf diesen
    private void SetNextPosition(Transform transform)
    {
        if (_movementCoroutine != null)
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

        _playerAnimator.SetBool("isWalking", true);

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
            // Empty = 3, Combat = 4, Loot = 5, Merchant = 6, Pond = 12, TownReached = 9
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
                _nextSceneIndex = ChooseInteractionType();

                break;

            case "Resting":
                // Beim ersten Tag immer in die Stadt führen, um diese zu zeigen und eh keine Conditios vorhandens sein können
                if (MainManager.Instance.CurrentDay == 0)
                {
                    _nextSceneIndex = 10;

                    break;
                }

                // Bosskampf nach dem letzten Tag
                if (MainManager.Instance.CurrentDay == _days.Length - 1)
                {
                    MainManager.Instance.CurrentDay++;

                    PlayerManager.Instance.HasReachedBoss = true;

                    _nextSceneIndex = 1;

                    break;
                }

                _nextSceneIndex = 9;

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

    private int ChooseInteractionType()
    {
        List<int> indexPool = new List<int> { 6, 12, 14 };
        return 14;
        if (MainManager.Instance.CurrentDay == 0)
        {
            return indexPool[2];
        }

        if (InventoryManager.Instance.InventoryItems.Count >= GameConfig.MaxInventorySlots)
        {
            indexPool.Remove(13);
        }

        if (ConditionManager.Instance.GetCurrentConditions().Count == 0)
        {
            indexPool.Remove(12);
        }

        if (indexPool.Count == 1)
        {
            return indexPool[0];
        }

        return indexPool[UnityEngine.Random.Range(0, indexPool.Count)];
    }
}
