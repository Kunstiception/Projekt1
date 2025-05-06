using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [SerializeField] protected string[] _texts;
    [SerializeField] protected TextMeshProUGUI _uiElement;
    [SerializeField] protected TextMeshProUGUI _promptSkip;
    [SerializeField] protected TextMeshProUGUI _promptContinue;
    protected Coroutine _textCoroutine;
    protected Coroutine _waitForContinueCoroutine;
    protected int _currentStringIndex = 0;
    protected string _currentLine;

    // Update is called once per frame
    void Update()
    {
        ListenForSkip();
    }

    protected void ListenForSkip()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(_textCoroutine != null)
            {
                _promptSkip.enabled = false;
                StopCoroutine(_textCoroutine);
                _textCoroutine = null;
                DialogueUtil.ShowFullLine(_currentLine, _uiElement, _promptSkip);
            }
        }
    }

    // Umfasst mehrere Methoden der Dalogue.Util-Klasse, händelt z.B. auch das Beenden eines Abschnitts wenn isLastLine == true
    protected IEnumerator HandleTextOutput(string line, bool isLastLine)
    {
        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _uiElement, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => line == _uiElement.text);

        if (isLastLine)
        {
            yield break;
        }

        // Einen Frame warten, damit Input nicht beide GetKeyDown-Events triggert
        yield return null;

        yield return StartCoroutine(DialogueUtil.WaitForContinue(_promptContinue));
    }

    public void ToggleCanvas(Canvas canvas, bool isActive)
    {
        canvas.enabled = isActive;

        var selectionMenu = canvas.GetComponent<SelectionMenu>();

        selectionMenu.SetInitialPointer();
        selectionMenu.enabled = isActive;

        if(isActive)
        {
            selectionMenu.InitializeMenu();
        }
    }
}
