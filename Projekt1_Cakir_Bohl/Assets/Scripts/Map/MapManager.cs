using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform[] _level1;
    [SerializeField] private Transform[] _level2;
    [SerializeField] private Transform[] _level3;
    
    private float _movementLength;
    private float _timer;
    private Coroutine _movementCoroutine;
    private Transform _nextPosition;
    
    void Start()
    {
        EvaluateCurrentPosition();
    }

    private void OnEnable()
    {      
        MapPoint.clickAction += GetNextPosition;
    }

    private void OnDisable()
    {
        MapPoint.clickAction -= GetNextPosition;
    }

    private void EvaluateCurrentPosition()
    {
        transform.position = _level1[0].position;
    }

    private void GetNextPosition(Transform transform)
    {
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
    }
}
