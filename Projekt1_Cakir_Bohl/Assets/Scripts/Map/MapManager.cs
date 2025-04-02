using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _firstWaypoint;

    private float _movementLength;
    private float _timer;
    private string _interactableTag = "Interactable";
    private string _nonInteractableTag = "Non_Interactable";
    private Transform _nextWaypoint;
    private Coroutine _movementCoroutine;
    
    private Transform _currentWaypoint;
    private List<Transform> _wayPoints = new List<Transform>();

    void Start()
    {
        GetWayPoints();
        SetCurrentPosition();
        SetWayPointsTag();
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

        foreach(var wayPoint in foundWayPoints)
        {
            _wayPoints.Add(wayPoint.transform);
            wayPoint.SetEnum(Random.Range(0, 3));
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

    private void SetWayPointsTag()
    {
        if(_nextWaypoint == null)
        {
            foreach(Transform wayPoint in _currentWaypoint.GetComponent<WayPoint>().adjacentWaypoints)
            {
                wayPoint.gameObject.tag = _interactableTag;
            }

            return;
        }

        foreach(Transform wayPoint in _nextWaypoint.GetComponent<WayPoint>().adjacentWaypoints)
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
        // Object id �ndert sich mit jeder Session, daher name
        MainManager.Instance.LastWayPoint = _currentWaypoint.name;
        MainManager.Instance.VisitedWayPoints.Add(_currentWaypoint.name);

        MainManager.Instance.SaveAll();

        SceneManager.LoadScene("CombatTest");
    }
}
