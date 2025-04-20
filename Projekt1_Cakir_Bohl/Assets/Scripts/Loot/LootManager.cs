using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LootManager : MonoBehaviour
{
    [SerializeField] private Item[] _possibleItems;
    [SerializeField] private TextMeshProUGUI _promptSkip;
    [SerializeField] private TextMeshProUGUI _promptContinue;
    [SerializeField] private TextMeshProUGUI _textBox;
    [SerializeField] private Canvas _selectionMenuCanvas;

    private Item _item;
    private string _currentLine;
    private Coroutine _textCoroutine;

    void Start()
    {
        int randomIndex = UnityEngine.Random.Range(0, _possibleItems.Length);
        _item = _possibleItems[randomIndex];
    }

    void Update()
    {
        // Ermöglicht sofortiges Anzeigen der gesamten derzeitigen Line
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_textCoroutine != null)
            {
                _promptSkip.enabled = false;
                StopCoroutine(_textCoroutine);
                _textCoroutine = null;
                DialogueUtil.ShowFullLine(_currentLine, _textBox, _promptSkip);
            }
        }
    }

    private IEnumerator HandleTextOutput(string line, bool isLastLine)
    {
        _textBox.enabled = true;
        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _textBox, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => _currentLine == _textBox.text);

        if (isLastLine)
        {          
            yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);
            _textBox.enabled = false;
            yield break;
        }

        // Einen Frame warten, damit Input nicht beide GetKeyDown-Events triggert
        yield return null;

        yield return StartCoroutine(DialogueUtil.WaitForContinue(_promptContinue));

        _textBox.enabled = false;
    }

    // Übergebenen Canvas und Skript an- oder ausschalten
    public void ToggleCanvas(Canvas canvas, bool isActive)
    {
        canvas.enabled = isActive;
        canvas.GetComponent<SelectionMenu>().enabled = isActive;
    }
}
