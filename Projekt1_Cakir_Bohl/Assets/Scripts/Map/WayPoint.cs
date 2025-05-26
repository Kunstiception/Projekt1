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
    [SerializeField] private GameObject[] _waypointVisuals;
    private string _interactableTag = "Interactable";
    private string _nonInteractableTag = "Non_Interactable";
    private string[] _possibleWayPointTypes = GameConfig.WayPointTypes;
    
    //https://gamedevbeginner.com/events-and-delegates-in-unity/
    public delegate void ClickAction(Transform transform);
    public static ClickAction clickAction;
    public static event Action OnClicked;

    private void Awake()
    {     
        foreach(GameObject sprite in _waypointVisuals)
        {
            sprite.SetActive(false);
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
        switch (type)
        {
            case 0:
                WayPointType = _possibleWayPointTypes[0];
                _waypointVisuals[0].SetActive(true);
                Debug.Log($"{gameObject.name}: I'm an empty waypoint");
                break;

            case 1:
                WayPointType = _possibleWayPointTypes[1];
                _waypointVisuals[1].SetActive(true);
                Debug.Log($"{gameObject.name}: I'm a fight waypoint");
                break;

            case 2:
                WayPointType = _possibleWayPointTypes[2];
                _waypointVisuals[2].SetActive(true);
                Debug.Log($"{gameObject.name}: I'm a loot waypoint");
                break;

            case 3:
                WayPointType = _possibleWayPointTypes[3];
                _waypointVisuals[3].SetActive(true);
                Debug.Log($"{gameObject.name}: I'm an interaction waypoint");
                break;

            case 4:
                WayPointType = _possibleWayPointTypes[4];
                _waypointVisuals[4].SetActive(true);
                Debug.Log($"{gameObject.name}: I'm a resting waypoint");
                break;

            default:
                Debug.Log($"{gameObject.name}: You guys have a type?");
                break;
        }
    }
}
