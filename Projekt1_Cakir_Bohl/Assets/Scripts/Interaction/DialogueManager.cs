using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// basiert auf: https://www.youtube.com/watch?v=vR6H3mu_xD8&list=PLSR2vNOypvs4Pc72kaB_Y1v3AszNd-UuF
public class DialogueManager : Manager, ISelectable
{
    [SerializeField] private DialogueLines _initialOptions;
    [SerializeField] private DialogueTopic[] _topics;
    [SerializeField] private GameObject _menuOptions;

    private DialogueLines _currentDialogueLines;
    private DialogueTopic _currentTopic;
    private TextMeshProUGUI[] _dialogueOptions;
    private Coroutine _dialogueCoroutine;
    private bool _isRunning;
    private Canvas _dialogueCanvas;

    void Start()
    {
        _dialogueOptions = _menuOptions.GetComponentsInChildren<TextMeshProUGUI>();

        _dialogueCanvas = _menuOptions.GetComponentInParent<Canvas>();
    }

    void Update()
    {
        ListenForSkip();
    }

    private IEnumerator DialogueCoroutine()
    {
        yield return ShowLinesAndHandleSelection(_initialOptions);
        
        do
        {
            yield return ShowLinesAndHandleSelection(_currentDialogueLines);

            if (_currentDialogueLines.positionInDialogue == PositionInDialogue.IsEnding)
            {
                yield return PrintMultipleLines(_currentDialogueLines.Lines);

                StopCoroutine(_dialogueCoroutine);

                _dialogueCoroutine = null;

                break;
            }

        } while (true);
    }

    private IEnumerator ShowLinesAndHandleSelection(DialogueLines dialogueLines)
    {
        ToggleCanvas(_dialogueCanvas, false);

        yield return PrintMultipleLines(dialogueLines.Lines);

        _textBox.text = "";

        _isRunning = false;

        for (int i = 0; i < _dialogueOptions.Length - 1; i++)
        {
            _dialogueOptions[i].text = dialogueLines.PlayerOptions[i];
        }

        ToggleCanvas(_dialogueCanvas, true);

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => _isRunning == true);

        ToggleCanvas(_dialogueCanvas, false);
    }

    // Reset to initial options without used option

    public void StartDialogue()
    {
        _dialogueCoroutine = StartCoroutine(DialogueCoroutine());
        _isRunning = true;
    }

    public void HandleSelectedMenuPoint(int index)
    {
        if (_currentDialogueLines == null)
            {
                _currentTopic = _topics[index];

                _currentDialogueLines = _initialOptions.BranchingLines[index];

                _isRunning = true;

                return;
            }

        // if (_currentDialogueLines.BranchingLines[index] == _initialOptions)
        // {
            
        // }

        _currentDialogueLines = _currentDialogueLines.BranchingLines[index];

        _isRunning = true;
    }
}
