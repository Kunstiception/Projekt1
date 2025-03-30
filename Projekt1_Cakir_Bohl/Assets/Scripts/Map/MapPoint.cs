using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPoint : MonoBehaviour
{
    //https://gamedevbeginner.com/events-and-delegates-in-unity/
    public delegate void ClickAction(Transform transform);
    public static ClickAction clickAction;

    private Color _originalColor;
    private Color _hoverColor = Color.red;

    void Start()
    {
        _originalColor = transform.GetComponent<Renderer>().material.color;
    }

    private void OnMouseEnter()
    {
        transform.GetComponent<Renderer>().material.color = _hoverColor;
    }

    private void OnMouseExit()
    {
        transform.GetComponent<Renderer>().material.color = _originalColor;
    }

    private void OnMouseDown()
    {
        clickAction?.Invoke(transform);
    }
}
