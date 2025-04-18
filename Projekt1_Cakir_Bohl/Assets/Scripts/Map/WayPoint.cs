using System;
using UnityEngine;

public class WayPoint : MonoBehaviour
{ 
        public enum IsRestingWayPoint
    {
        IsResting  = 0,
        IsAction  = 1
    }
    
    //https://gamedevbeginner.com/events-and-delegates-in-unity/
    public delegate void ClickAction(Transform transform);
    public static ClickAction clickAction;

    public static event Action OnClicked;

    public string WayPointType;
    [SerializeField] public Transform[] AdjacentWaypoints;
    private Color _originalColor;
    private Color _hoverColor = Color.red;
    private string _interactableTag = "Interactable";
    private string _nonInteractableTag = "Non_Interactable";
    private string[] _possibleWayPointTypes = GameConfig.WayPointTypes;

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
            default:
                Debug.Log($"{gameObject.name}: You guys have a type?");
                break;
        }
    }
}
