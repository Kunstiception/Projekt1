using System;
using UnityEngine;

public class WayPoint : MonoBehaviour
{ 
    public enum WayPointCategory
    {
        IsResting  = 0,
        IsAction  = 1,
        IsStart = 2
    }
    
    public string WayPointType;
    public WayPointCategory wayPointCategory;
    [SerializeField] public Transform[] AdjacentWaypoints;
    [SerializeField] private SpriteRenderer[] _waypointVisuals;
    private string _interactableTag = "Interactable";
    private string _nonInteractableTag = "Non_Interactable";
    private string[] _possibleWayPointTypes = GameConfig.WayPointTypes;
    
    //https://gamedevbeginner.com/events-and-delegates-in-unity/
    public delegate void ClickAction(Transform transform);
    public static ClickAction clickAction;
    public static event Action OnClicked;

    private void Awake()
    {     
        foreach(SpriteRenderer sprite in _waypointVisuals)
        {
            sprite.enabled = false;
        }
    }

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
        if(wayPointCategory == WayPointCategory.IsStart)
        {
            WayPointType = _possibleWayPointTypes[0];
            _waypointVisuals[0].enabled = true;
            Debug.Log($"{gameObject.name}: I'm an empty waypoint");

            return;
        }
        
        switch (type)
        {
            case 0:
                WayPointType = _possibleWayPointTypes[0];
                _waypointVisuals[0].enabled = true;
                Debug.Log($"{gameObject.name}: I'm an empty waypoint");
                break;

            case 1:
                WayPointType = _possibleWayPointTypes[1];
                _waypointVisuals[1].enabled = true;
                Debug.Log($"{gameObject.name}: I'm a fight waypoint");
                break;

            case 2:
                WayPointType = _possibleWayPointTypes[2];
                _waypointVisuals[2].enabled = true;
                Debug.Log($"{gameObject.name}: I'm a loot waypoint");
                break;

            case 3:
                WayPointType = _possibleWayPointTypes[3];
                _waypointVisuals[3].enabled = true;
                Debug.Log($"{gameObject.name}: I'm an interaction waypoint");
                break;

            case 4:
                WayPointType = _possibleWayPointTypes[4];
                _waypointVisuals[4].enabled = true;
                Debug.Log($"{gameObject.name}: I'm a resting waypoint");
                break;

            default:
                Debug.Log($"{gameObject.name}: You guys have a type?");
                break;
        }
    }
}
