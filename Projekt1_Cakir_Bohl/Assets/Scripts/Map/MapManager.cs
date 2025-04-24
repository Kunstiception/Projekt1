using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _firstWaypoint;
    [SerializeField] private SceneAsset _emptyScene;
    [SerializeField] private SceneAsset _fightScene;
    [SerializeField] private SceneAsset _lootScene;
    [SerializeField] private SceneAsset _interactionScene;
    [SerializeField] private SceneAsset _restingScene;

    private float _movementLength;
    private float _timer;
    private string _interactableTag = "Interactable";
    private string _nonInteractableTag = "Non_Interactable";
    private string _nextScene;
    private Transform _nextWaypoint;
    private Coroutine _movementCoroutine;
    private Transform _currentWaypoint;
    private List<Transform> _wayPoints = new List<Transform>();

    void Start()
    {
        // Public string nie null
        if(MainManager.Instance.LastWayPoint.Length != 0 && !MainManager.Instance.VisitedWayPoints.Contains(MainManager.Instance.LastWayPoint))
        {
            MainManager.Instance.VisitedWayPoints.Add(MainManager.Instance.LastWayPoint);

            MainManager.Instance.SaveAll();
        }
        else if(PlayerManager.Instance != null)
        {
            PlayerManager.Instance.InitializePlayerStats();
        }

        GetWayPoints();
        SetCurrentPosition();
        SetNextWayPointsTag();      
    }

    private void OnEnable()
    {      
        WayPoint.clickAction += SetNextPosition;
    }

    private void OnDisable()
    {
        WayPoint.clickAction -= SetNextPosition;
    }

    private void GetWayPoints()
    {
        var foundWayPoints = FindObjectsByType<WayPoint>(FindObjectsSortMode.None);

        // 0 = empty, 1 = fight, 2 = loot, 3 = interaction, 4 = resting
        foreach(var wayPoint in foundWayPoints)
        {
            _wayPoints.Add(wayPoint.transform);
            //https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.contains?view=net-9.0
            if(MainManager.Instance.VisitedWayPoints.Contains(wayPoint.gameObject.transform.name))
            {
                wayPoint.SetType(0);
                continue;
            }

            if(wayPoint.isRestingWayPoint == 0)
            {
                wayPoint.SetType(4);
                continue;
            }

            wayPoint.SetType(Random.Range(2, 3));
        }
    }

    private void SetCurrentPosition()
    {
        if(MainManager.Instance == null)
        {
            return;
        }
        
        //public string scheinbar nie null?
        if(MainManager.Instance.LastWayPoint.Length == 0)
        {
            _currentWaypoint = _firstWaypoint;
        }
        else
        {
            _currentWaypoint = GameObject.Find(MainManager.Instance.LastWayPoint).transform;
        }

        _player.position = _currentWaypoint.position;
    }

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

    private void SetNextPosition(Transform transform)
    {
        if(_movementCoroutine != null)
        {
            return;
        }

        _nextWaypoint = transform;
        _movementCoroutine = StartCoroutine(MovementCoroutine(_player.position, _nextWaypoint.position));
    }

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
            case "Empty":
                _nextScene = _emptyScene.name;
                break;
            case "Fight":
                _nextScene = _fightScene.name;
                break;
            case "Loot":
                _nextScene = _lootScene.name;
                break;
            case "Interaction":
                _nextScene = _interactionScene.name;
                break;
            case "Resting":
                _nextScene = _restingScene.name;
                break;
            default:
                Debug.LogError("No scene found!");
                break;
        }

        // Object id ändert sich mit jeder Session, daher name
        MainManager.Instance.LastWayPoint = _currentWaypoint.name;

        SceneManager.LoadScene(_nextScene);
    }
}
