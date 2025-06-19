using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextManager : Manager
{
    [TextArea]
    [SerializeField] private string[] _textDay0;
    [TextArea]
    [SerializeField] private string[] _textDay1;
    [TextArea]
    [SerializeField] private string[] _textDay2;
    [TextArea]
    [SerializeField] private string[] _textDay3;
    [TextArea]
    [SerializeField] private string[] _textDay4;
    [TextArea]
    [SerializeField] private string[] _textDay5;

    private Dictionary<int, string[]> _texts = new Dictionary<int, string[]>();

    void Start()
    {
        _promptContinue.enabled = false;

        StartCoroutine(Dialogue(_textDay0));
    }

    void Update()
    {
        ListenForSkip();
    }

    private IEnumerator Dialogue(string[] strings)
    {
        for (int i = 0; i < strings.Length; i++)
        {
            if (!_promptSkip.isActiveAndEnabled)
            {
                _promptSkip.enabled = true;
            }
            _currentLine = strings[i];

            if(i != strings.Length - 1)
            {
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
            else
            {
                yield return StartCoroutine(HandleTextOutput(_currentLine, true));
            }
        }

        yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);

        SceneManager.LoadScene(2);
    }
}
