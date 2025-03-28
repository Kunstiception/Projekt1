using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _uiElement;
    [SerializeField] private TextMeshProUGUI _promptSkip;
    [SerializeField] private TextMeshProUGUI _promptContinue;

    private Coroutine _textCoroutine;
    private int _currentString = 0;
    private string[] _text = { "Hello, World! Witness the glory of Projekt 1!", 
        "Created by Zara Cakir and Lukas Bohl.", 
        "Let us send the hero on his very first quest!" };
    
    void Start()
    {
        StartCoroutine(Dialogue());

        _promptContinue.enabled = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(_textCoroutine != null)
            {
                StopCoroutine(_textCoroutine);

                _textCoroutine = null;
                _promptSkip.enabled = false;

                DialogueUtil.ShowFullLine(_text[_currentString], _uiElement);

                _currentString++;
            }
        }
    }

    IEnumerator Dialogue()
    {
        while(true)
        {
            for (int i = 0; i < _text.Length; i++)
            {
                if (!_promptSkip.isActiveAndEnabled)
                {
                    _promptSkip.enabled = true;
                }

                _currentString = i;

                yield return _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(_text[_currentString], _uiElement));

                _promptSkip.enabled = false;
                _promptContinue.enabled = true;

                while (!Input.GetKeyDown(KeyCode.Space))
                {
                    yield return null;
                }

                _promptContinue.enabled = false;
                _promptSkip.enabled = true;
            }
        }
    }
}
