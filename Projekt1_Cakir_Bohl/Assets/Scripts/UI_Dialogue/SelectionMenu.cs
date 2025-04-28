using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMenu : MonoBehaviour
{
    public enum MenuLayer
    {
        isVertical = 0,
        isHorizontal  = 1
    }

    public MenuLayer menuLayer;
       
    [SerializeField] protected TextMeshProUGUI[] _menuPoints;
    [SerializeField] protected Image[] _pointers;
    [SerializeField] protected GameObject _connectedScript;

    protected int _currentMenuPoint;
    private bool _isVertical;
    private ISelectable _iSelectable;

    void Start()
    {
        _isVertical = menuLayer == MenuLayer.isVertical ? true : false;
        _iSelectable = _connectedScript.GetComponent<ISelectable>();
        SetInitialPointer();
    }

    protected void OnEnable()
    {
        CombatManager.OnFightFinished += SetInitialPointer;
    }

    protected void OnDisable()
    {
        CombatManager.OnFightFinished -= SetInitialPointer;
    }

    // Update is called once per frame
    void Update()
    {
        ListenForInputs();
    }

    public virtual void ListenForInputs()
    {
        if(_isVertical)
        {
            if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if(_currentMenuPoint == _menuPoints.Length - 1)
                {
                    return;
                }

                ChangePosition(isUpOrLeft: false);
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                if (_currentMenuPoint == 0)
                {
                    return;
                }

                ChangePosition(isUpOrLeft: true);
            }
            else if(Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                _iSelectable.HandleSelectedMenuPoint(_currentMenuPoint, isFirstLayer: _isVertical);
            }
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if(_currentMenuPoint == _menuPoints.Length - 1)
                {
                    return;
                }

                ChangePosition(isUpOrLeft: false);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                if (_currentMenuPoint == 0)
                {
                    return;
                }

                ChangePosition(isUpOrLeft: true);
            }
            else if(Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                HandleSelection(_currentMenuPoint);
            }
        }      
    }

    public virtual void HandleSelection(int menuPoint)
    {
        _iSelectable.HandleSelectedMenuPoint(menuPoint, isFirstLayer: _isVertical);
    }

    public void SetInitialPointer()
    {
        if (_menuPoints[0] == null || _pointers[0] == null)
        {
            Debug.LogError("Missing Menu Components!");
            return;
        }

        _currentMenuPoint = 0;
        _pointers[_currentMenuPoint].gameObject.SetActive(true);
        
        for(int i = 1;  i < _pointers.Length; i++)
        {
            _pointers[i].gameObject.SetActive(false);
        }
    }

    public virtual void ChangePosition(bool isUpOrLeft)
    {
        if(isUpOrLeft)
        {
            _currentMenuPoint--;
        }
        else
        {
            _currentMenuPoint++;
        }

        for (int i = 0; i < _pointers.Length; i++)
        {
            if(i == _currentMenuPoint)
            {
                _pointers[i].gameObject.SetActive(true);
                continue;
            }

            _pointers[i].gameObject.SetActive(false);
        }
    }
}