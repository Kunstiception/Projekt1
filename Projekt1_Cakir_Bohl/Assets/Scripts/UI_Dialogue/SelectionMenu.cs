using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _menuPoints;
    [SerializeField] private Image[] _pointers;

    private int _currentMenuPoint;

    void Start()
    {
        SetInitialPointer();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(_currentMenuPoint == _menuPoints.Length - 1)
            {
                return;
            }

            ChangePosition(isUp: false);
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
            if (_currentMenuPoint == 0)
            {
                return;
            }

            ChangePosition(isUp: true);
        }
    }

    private void SetInitialPointer()
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

    private void ChangePosition(bool isUp)
    {
        if(isUp)
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

        print(_currentMenuPoint);
    }
}
