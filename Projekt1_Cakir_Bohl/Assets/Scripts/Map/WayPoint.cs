using System;
using UnityEngine;

public class WayPoint : MonoBehaviour
{ 
    public enum IsRestingWayPoint
    {
        IsResting  = 0,
        IsAction  = 1
    }
    
    public string WayPointType;
    public IsRestingWayPoint isRestingWayPoint;
    [SerializeField] public Transform[] AdjacentWaypoints;
    private string _interactableTag = "Interactable";
    private string _nonInteractableTag = "Non_Interactable";
    private string[] _possibleWayPointTypes = GameConfig.WayPointTypes;
    
    //https://gamedevbeginner.com/events-and-delegates-in-unity/
    public delegate void ClickAction(Transform transform);
    public static ClickAction clickAction;
    public static event Action OnClicked;

    private void OnEnable()
    {
        OnClicked += SetTag;
    }

    private void OnDisable()
    {
        OnClicked -= SetTag;
    }

    private void OnMouseEnter()
    {
        if (gameObject.CompareTag(_interactableTag))
        {
            //transform.GetComponent<Renderer>().material.color = _hoverColor;
        }
    }

    private void OnMouseExit()
    {
        if (gameObject.CompareTag(_interactableTag))
        {
            //transform.GetComponent<Renderer>().material.color = _originalColor;
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

    private void SetTag()
    {
        gameObject.tag = _nonInteractableTag;
    }

    public void SetType(int type)
    {
        switch (type)
        {
            case 0:
                WayPointType = _possibleWayPointTypes[0];
                Debug.Log($"{gameObject.name}: I'm an empty waypoint");
                break;

            case 1:
                WayPointType = _possibleWayPointTypes[1];
                Debug.Log($"{gameObject.name}: I'm a fight waypoint");
                break;

            case 2:
                WayPointType = _possibleWayPointTypes[2];
                Debug.Log($"{gameObject.name}: I'm a loot waypoint");
                break;

            case 3:
                WayPointType = _possibleWayPointTypes[3];
                Debug.Log($"{gameObject.name}: I'm an interaction waypoint");
                break;

            case 4:
                WayPointType = _possibleWayPointTypes[4];
                Debug.Log($"{gameObject.name}: I'm a resting waypoint");
                break;

            default:
                Debug.Log($"{gameObject.name}: You guys have a type?");
                break;
        }
    }
}
