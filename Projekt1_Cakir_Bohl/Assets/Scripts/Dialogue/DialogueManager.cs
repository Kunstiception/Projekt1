using System.Collections;
using TMPro;
using UnityEngine;

// basiert auf: https://www.youtube.com/watch?v=vR6H3mu_xD8&list=PLSR2vNOypvs4Pc72kaB_Y1v3AszNd-UuF
public class DialogueManager : Manager, ISelectable
{
    [SerializeField] public DialogueLines InitialOptions;
    [SerializeField] private GameObject _followingMenuOptions;
    [SerializeField] private GameObject _initialMenuOptions;

    private DialogueLines _currentDialogueLines;
    private TextMeshProUGUI[] _dialogueOptions;
    private Coroutine _dialogueCoroutine;
    private bool _isRunning;
    private Canvas _dialogueCanvas;

    public delegate void OnDialogueFinished();
    public static OnDialogueFinished onDialogueFinished;

    void Start()
    {
        _dialogueCanvas = _initialMenuOptions.GetComponentInParent<Canvas>();

        ToggleMenus(true);
    }

    void Update()
    {
        ListenForSkip();
    }

    private IEnumerator DialogueCoroutine()
    {
        yield return ShowLinesAndHandleSelection(InitialOptions);

        do
        {
            if (_currentDialogueLines.positionInDialogue == PositionInDialogue.IsEnding)
            {
                yield return PrintMultipleLines(_currentDialogueLines.Lines);

                EndDialogue();

                break;
            }

            yield return ShowLinesAndHandleSelection(_currentDialogueLines);

        } while (_isRunning && _textBox.text != _currentLine);
    }

    private IEnumerator ShowLinesAndHandleSelection(DialogueLines dialogueLines)
    {
        ToggleCanvas(_dialogueCanvas, false);

        yield return PrintMultipleLines(dialogueLines.Lines);

        _textBox.text = "";

        _isRunning = false;

        if (dialogueLines == InitialOptions)
        {
            ToggleMenus(true);
        }
        else
        {
            ToggleMenus(false);
        }

        for (int i = 0; i < _dialogueOptions.Length; i++)
        {
            _dialogueOptions[i].text = dialogueLines.PlayerOptions[i];
        }

        ToggleCanvas(_dialogueCanvas, true);

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => _isRunning == true);

        ToggleCanvas(_dialogueCanvas, false);
    }

    public void StartDialogue()
    {
        _dialogueCoroutine = StartCoroutine(DialogueCoroutine());
        _isRunning = true;
        _currentDialogueLines = InitialOptions;
    }

    public DialogueLines ReturnCurrentDialogueLines()
    {
        return _currentDialogueLines;
    }

    public void HandleSelectedMenuPoint(int index)
    {
        if (_currentDialogueLines == null)
        {
            _currentDialogueLines = InitialOptions;

            _isRunning = true;

            return;
        }

        _currentDialogueLines = _currentDialogueLines.BranchingLines[index];

        if (_currentDialogueLines == InitialOptions)
        {
            EndDialogue();

            return;
        }

        _isRunning = true;
    }

    private void EndDialogue()
    {
        StopCoroutine(_dialogueCoroutine);

        onDialogueFinished?.Invoke();

        _isRunning = false;
        _textBox.text = "";
    }

    private void ToggleMenus(bool isInitialMenu)
    {
        SelectionMenu _nextSelectionMenu;

        if (isInitialMenu)
        {
            _initialMenuOptions.SetActive(true);
            _nextSelectionMenu = _initialMenuOptions.GetComponent<SelectionMenu>();
            _nextSelectionMenu.InitializeMenu();

            _followingMenuOptions.SetActive(false);

            _dialogueOptions = _initialMenuOptions.GetComponentsInChildren<TextMeshProUGUI>();
        }
        else
        {
            _followingMenuOptions.SetActive(true);
            _nextSelectionMenu = _followingMenuOptions.GetComponent<SelectionMenu>();
            _nextSelectionMenu.InitializeMenu();


            _initialMenuOptions.SetActive(false);
            // _initialMenuOptions.GetComponent<SelectionMenu>().IsActive = false;

            _dialogueOptions = _followingMenuOptions.GetComponentsInChildren<TextMeshProUGUI>();
        }
    }
}