using System;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{ 
    //https://gamedevbeginner.com/events-and-delegates-in-unity/
    public delegate void ClickAction(Transform transform);
    public static ClickAction clickAction;

    public static event Action OnClicked;

    [SerializeField] public int type;
    [SerializeField] public Transform[] adjacentWaypoints;

    private Color _originalColor;
    private Color _hoverColor = Color.red;
    private string _interactableTag = "Interactable";
    private string _nonInteractableTag = "Non_Interactable";
    private string[] _possibleWayPointTypes = GameConfig.WayPointTypes;
    private string _wayPointType;

    void Start()
    {
        _originalColor = transform.GetComponent<Renderer>().material.color;
    }

    private void OnEnable()
    {
        OnClicked += SetColorAndTag;
    }

    private void OnDisable()
    {
        OnClicked -= SetColorAndTag;
    }

    private void OnMouseEnter()
    {
        if (gameObject.CompareTag(_interactableTag))
        {
            transform.GetComponent<Renderer>().material.color = _hoverColor;
        }
    }

    private void OnMouseExit()
    {
        if (gameObject.CompareTag(_interactableTag))
        {
            transform.GetComponent<Renderer>().material.color = _originalColor;
        }
    }

    private void OnMouseDown()
    {
        if (gameObject.CompareTag(_interactableTag))
        {
            clickAction?.Invoke(transform);
            OnClicked?.Invoke();
        }
    }

    private void SetColorAndTag()
    {
        gameObject.tag = _nonInteractableTag;
        transform.GetComponent<Renderer>().material.color = _originalColor;
    }

    public void SetEnum(int type)
    {
        switch (type)
        {
            case 0:
                _wayPointType = _possibleWayPointTypes[0];
                Debug.Log("I'm an empty waypoint");
                break;
            case 1:
                _wayPointType = _possibleWayPointTypes[0];
                Debug.Log("I'm a fight waypoint");
                break;
            case 2:
                _wayPointType = _possibleWayPointTypes[0];
                Debug.Log("I'm a loot waypoint");
                break;
            case 3:
                _wayPointType = _possibleWayPointTypes[0];
                Debug.Log("I'm an interaction waypoint");
                break;
            default:
                Debug.Log("You guys have a type?");
                break;
        }
    }
}
