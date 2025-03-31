using System;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    //https://gamedevbeginner.com/events-and-delegates-in-unity/
    public delegate void ClickAction(Transform transform);
    public static ClickAction clickAction;

    public static event Action OnClicked;

    [SerializeField] public int level;

    private Color _originalColor;
    private Color _hoverColor = Color.red;
    private string _interactableTag = "Interactable";
    private string _nonInteractableTag = "Non_Interactable";

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
}
