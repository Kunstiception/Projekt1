using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Manager
{
    void Start()
    {
        _promptContinue.enabled = false;

        StartCoroutine(Dialogue(_texts));
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
