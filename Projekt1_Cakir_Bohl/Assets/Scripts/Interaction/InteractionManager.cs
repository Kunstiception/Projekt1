using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractionManager : Manager, ISelectable
{
    [SerializeField] public Canvas MerchantInventoryCanvas;
    [SerializeField] public Canvas ItemToDoCanvas;
    [SerializeField] public Canvas InitialMenuCanvas;
    [SerializeField] public Canvas DialogueCanvas;
    [SerializeField] private AudioClip _merchantEntrance;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private GameObject _merchant;

    private IEnumerator Start()
    {
        ToggleCursorState(true);
        ToggleCanvas(MerchantInventoryCanvas, false);
        ToggleCanvas(ItemToDoCanvas, false);
        ToggleCanvas(InitialMenuCanvas, false);
        ToggleCanvas(DialogueCanvas, false);

        InitializePlayerStats();

        _merchant.SetActive(true);

        _textBox.enabled = true;
        _promptSkip.enabled = false;
        _promptContinue.enabled = false;

        yield return StartCoroutine(EvaluateVampire());

        _audioSource.PlayOneShot(_merchantEntrance);

        _currentLine = "Feel free to take a look at my merchandise, dear knight.";
        yield return StartCoroutine(HandleTextOutput(_currentLine, false));

        _textBox.text = "";

        ToggleCanvas(InitialMenuCanvas, true);
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

    public void HandleSelectedMenuPoint(int index)
    {
        switch (index)
        {
            case 0:
                ToggleCanvas(MerchantInventoryCanvas, true);
                ToggleCanvas(InitialMenuCanvas, false);

                break;

            case 1:
                ToggleCanvas(InitialMenuCanvas, false);
                
                if (ConditionManager.Instance.IsZombie)
                {
                    StartCoroutine(ZombieConversationAttempt());

                    return;
                }

                ToggleCanvas(DialogueCanvas, true);

                _dialogueManager.StartDialogue();

                break;

            case 2:
                SceneManager.LoadScene(2);

                break;
        }
    }

    private void ResetMenus()
    {
        ToggleCanvas(InitialMenuCanvas, true);
        ToggleCanvas(DialogueCanvas, false);
    }
    
    public override IEnumerator ZombieConversationAttempt()
    {
        yield return base.ZombieConversationAttempt();

        ToggleCanvas(InitialMenuCanvas, true);
    }
}
