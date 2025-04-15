using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string[] _texts;
    [SerializeField] private string _nextScene;
    [SerializeField] private TextMeshProUGUI _uiElement;
    [SerializeField] private TextMeshProUGUI _promptSkip;
    [SerializeField] private TextMeshProUGUI _promptContinue;

    private Coroutine _textCoroutine;
    private Coroutine _waitForContinueCoroutine;
    private int _currentStringIndex = 0;
    
    void Start()
    {
        _promptContinue.enabled = false;

        StartCoroutine(Dialogue(_texts));
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(_textCoroutine != null)
            {
                _promptSkip.enabled = false;
                StopCoroutine(_textCoroutine);
                _textCoroutine = null;
                DialogueUtil.ShowFullLine(_texts[_currentStringIndex], _uiElement, _promptSkip);
            }
        }
    }

    IEnumerator Dialogue(string[] strings)
    {
        for (int i = 0; i < strings.Length; i++)
        {
            if (!_promptSkip.isActiveAndEnabled)
            {
                _promptSkip.enabled = true;
            }

            _currentStringIndex = i;

            if(i != strings.Length - 1)
            {
                yield return StartCoroutine(HandleTextOutput(strings[_currentStringIndex], false));
            }
            else
            {
                yield return StartCoroutine(HandleTextOutput(strings[_currentStringIndex], true));
            }
        }

        yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);

        SceneManager.LoadScene(_nextScene);
    }

    private IEnumerator HandleTextOutput(string line, bool isLastLine)
    {
        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _uiElement, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => _texts[_currentStringIndex] == _uiElement.text);

        if (isLastLine)
        {
            yield break;
        }

        // Einen Frame warten, damit Input nicht beide GetKeyDown-Events triggert
        yield return null;

        yield return StartCoroutine(DialogueUtil.WaitForContinue(_promptContinue));
    }
}
