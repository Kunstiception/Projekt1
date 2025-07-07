using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DogManager : Manager, ISelectable
{
    [SerializeField] private Canvas _initialSelectionMenuCanvas;
    [SerializeField] private Canvas _dialogueCanvas;
    [SerializeField] private DialogueLines _firstMeetingLines;
    [SerializeField] private DialogueLines _secondMeetingLines;
    [SerializeField] private Item[] _possibleItems;
    [SerializeField] private DialogueManager _dialogueManager;

    IEnumerator Start()
    {
        ToggleCursorState(true);

        ToggleCanvas(_initialSelectionMenuCanvas, false);
        ToggleCanvas(_dialogueCanvas, false);

        InitializePlayerStats();

        yield return EvaluateVampire();

        if (MainManager.Instance.CurrentDay == 0)
        {
            _dialogueManager.InitialOptions = _firstMeetingLines;
        }
        else
        {
            _dialogueManager.InitialOptions = _secondMeetingLines;
        }

        if (!MainManager.Instance.HasBefriendedDog)
        {
            if (MainManager.Instance.CurrentDay == 0)
            {
                yield return PrintMultipleLines(UIDialogueStorage.MeetingDogLines);
            }
            else
            {
                yield return PrintMultipleLines(UIDialogueStorage.MeetingDogAgainLines);
            }

            _textBox.text = "";

            ResetMenus();
        }
        else
        {
            StartCoroutine(GiveItem());
        }
    }

    private void OnEnable()
    {
        DialogueManager.onDialogueFinished += ResetMenus;
    }

    private void OnDisable()
    {
        DialogueManager.onDialogueFinished -= ResetMenus;

        StopAllCoroutines();
    }

    void Update()
    {
        ListenForSkip();
    }

    private void ResetMenus()
    {
        ToggleCanvas(_initialSelectionMenuCanvas, true);
        ToggleCanvas(_dialogueCanvas, false);
    }

    private IEnumerator GiveItem()
    {
        int randomIndex = UnityEngine.Random.Range(0, _possibleItems.Length);

        Item item = _possibleItems[randomIndex];

        _currentLine = $"Look! The dog brings you {item.Name}";
        yield return HandleTextOutput(_currentLine, false);

        InventoryManager.Instance.ManageInventory(item, 1, true);

        SceneManager.LoadScene(2);

        yield break;
    }

    public void HandleSelectedMenuPoint(int index)
    {
        if (!_initialSelectionMenuCanvas.isActiveAndEnabled)
        {
            return;
        }

        ToggleCanvas(_initialSelectionMenuCanvas, false);
        
        switch (index)
        {
            case 0:
                StartCoroutine(PetCoroutine());

                break;

            case 1:
                ToggleCanvas(_dialogueCanvas, true);

                _dialogueManager.StartDialogue();

                break;

            case 2:
                StartCoroutine(LeavingCoroutine());

                break;
        }
    }

    private IEnumerator PetCoroutine()
    {
        if (!MainManager.Instance.HasBefriendedDog)
        {
            yield return PrintMultipleLines(UIDialogueStorage.PetDogLines);
        }
        else
        {
            yield return PrintMultipleLines(UIDialogueStorage.PetDogAgainLines);
        }

        _textBox.text = "";

        ResetMenus();
    }

    private IEnumerator LeavingCoroutine()
    {
        _currentLine = "You leave the dog behind and continue your quest.";
        yield return HandleTextOutput(_currentLine, false);

        SceneManager.LoadScene(2);
    }
}
