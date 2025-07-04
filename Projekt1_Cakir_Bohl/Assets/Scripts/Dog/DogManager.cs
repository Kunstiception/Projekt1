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

    void Start()
    {

        if (MainManager.Instance.CurrentDay == 0)
        {
            _dialogueManager.InitialOptions = _firstMeetingLines;

            return;
        }
        else
        {
            _dialogueManager.InitialOptions = _secondMeetingLines;
        }

        if (!MainManager.Instance.HasBefriendedDog)
        {
            ResetMenus();
        }
        else
        {
            StartCoroutine(GiveItem());

            ToggleCanvas(_initialSelectionMenuCanvas, false);
            ToggleCanvas(_dialogueCanvas, false);
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
        switch (index)
        {
            case 0:
                // Pet Coroutine

                break;

            case 1:
                _dialogueManager.StartDialogue();

                break;

            case 2:
                // Leaving Coroutine

                break;
        }
    }

    // Pet Coroutine
    
    // Leaving Coroutine

}
