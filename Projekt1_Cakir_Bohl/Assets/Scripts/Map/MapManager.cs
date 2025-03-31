using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    
    
    [SerializeField] private Transform _player;

    private float _movementLength;
    private float _timer;
    private string _interactableTag = "Interactable";
    private string _nonInteractableTag = "Non_Interactable";
    private Transform _nextPosition;
    private Coroutine _movementCoroutine;
    
    private KeyValuePair<Transform, int> _currentPositionAndLevel;
    private Dictionary<Transform, int> _wayPoints = new Dictionary<Transform, int>();

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
            _wayPoints.Add(wayPoint.transform, wayPoint.level);
        }
    }

    private void SetCurrentPosition()
    {
        if (_currentPositionAndLevel.Key == null)
        {
            //https://stackoverflow.com/questions/2444033/get-dictionary-key-by-value
            _currentPositionAndLevel = new KeyValuePair<Transform, int>(_wayPoints.FirstOrDefault(x => x.Value == 0).Key, 0);
        }

        _player.position = _currentPositionAndLevel.Key.position;
    }

    private void IncrementCurrentPosition()
    {
        _currentPositionAndLevel = new KeyValuePair<Transform, int>(_nextPosition, _currentPositionAndLevel.Value + 1);

    }

    private void SetWayPointsTag()
    {
        foreach(var wayPoint in _wayPoints)
        {
            if(wayPoint.Value == _currentPositionAndLevel.Value + 1)
            {
                wayPoint.Key.gameObject.tag = _interactableTag;
            }
            else
            {
                wayPoint.Key.gameObject.tag = _nonInteractableTag;
            }
        }
    }

    private void SetNextPosition(Transform transform)
    {
        if(_movementCoroutine != null)
        {
            return;
        }

        _nextPosition = transform;
        _movementCoroutine = StartCoroutine(MovementCoroutine(_player.position, _nextPosition.position));
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
        
        IncrementCurrentPosition();
        SetWayPointsTag();
    }
}
