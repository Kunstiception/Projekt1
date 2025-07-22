using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DogManager : Manager, ISelectable
{
    [SerializeField] private Canvas _initialSelectionMenuCanvas;
    [SerializeField] private Canvas _dialogueCanvas;
    [SerializeField] private DialogueLines _firstMeetingLines;
    [SerializeField] private DialogueLines _secondMeetingLines;
    [SerializeField] private DialogueLines[] _hasBefriendedLines;
    [SerializeField] private Item[] _possibleItems;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private AudioClip _dogEntrance;

    IEnumerator Start()
    {
        SetPrompts();

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

        _audioSource.PlayOneShot(_dogEntrance);

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
        DialogueManager.onDialogueFinished += CheckForFriend;
    }

    private void OnDisable()
    {
        DialogueManager.onDialogueFinished -= ResetMenus;
        DialogueManager.onDialogueFinished -= CheckForFriend;

        StopAllCoroutines();
    }

    private void CheckForFriend()
    {
        foreach (DialogueLines lines in _hasBefriendedLines)
        {
            if (lines == _dialogueManager.ReturnCurrentDialogueLines())
            {
                MainManager.Instance.HasBefriendedDog = true;

                return;
            }
        }
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

        _currentLine = $"Look! The dog brings you something!";
        yield return HandleTextOutput(_currentLine, false);

        _currentLine = $"You have received {item.Name}.";
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
                if (ConditionManager.Instance.IsZombie)
                {
                    StartCoroutine(ZombieConversationAttempt());

                    return;
                }

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
        if (MainManager.Instance.CurrentDay == 0)
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

    public override IEnumerator ZombieConversationAttempt()
    {
        yield return base.ZombieConversationAttempt();

        ResetMenus();
    }
    
    // public override void ListenForSkipOrAuto()
    // {
    //     if (!_dialogueCanvas.isActiveAndEnabled)
    //     {
    //         base.ListenForSkipOrAuto();
    //     }
    // }
}
