using System.Collections;
using TMPro;
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
    private int _currentString = 0;
    
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
                StopCoroutine(_textCoroutine);

                _textCoroutine = null;
                _promptSkip.enabled = false;

                DialogueUtil.ShowFullLine(_texts[_currentString], _uiElement);
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

            _currentString = i;

            _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(strings[_currentString], _uiElement));

            //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
            yield return new WaitUntil(() => strings[_currentString] == _uiElement.text);

            _promptSkip.enabled = false;

            // wait one frame to not trigger following GetKeyDown event
            yield return new WaitForSeconds(GameConfig.TimeBeforeNextLine);

            if(_currentString == strings.Length - 1)
            {
                break;
            }

            _promptContinue.enabled = true;

            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }

            _promptContinue.enabled = false;
            _promptSkip.enabled = true;
        }

        yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);

        SceneManager.LoadScene(_nextScene);
    }
}
