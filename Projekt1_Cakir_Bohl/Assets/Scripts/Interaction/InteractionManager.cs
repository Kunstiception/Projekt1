using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Dieses Skript händelt die Basis-Funktionen der Merchant-Szene insofern man nicht einkauft oder mit ihr spricht
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

        SetPrompts();

        yield return StartCoroutine(EvaluateVampire());

        _mainEffectsAudioSource.PlayOneShot(_merchantEntrance);

        _textBox.enabled = true;

        _currentLine = "Merchant: 'Feel free to take a look at my merchandise, dear knight.'";
        yield return StartCoroutine(HandleTextOutput(_currentLine, false, true));

        _textBox.text = "";

        ToggleCanvas(InitialMenuCanvas, true);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        DialogueManager.OnDialogueFinished += ResetMenus;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        DialogueManager.OnDialogueFinished -= ResetMenus;

        StopAllCoroutines();
    }

    // Nur auf Eingabe prüfen, wenn kein Dialog und kein Shopping
    void Update()
    {
        if (PlayerManager.Instance.IsTalking || MerchantInventoryCanvas.isActiveAndEnabled)
        {
            return;
        }

        ListenForSkipOrAuto();
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

                PlayerManager.Instance.IsTalking = true;

                break;

            case 2:
                SceneManager.LoadScene(2);

                break;
        }
    }

    private void ResetMenus()
    {
        PlayerManager.Instance.IsTalking = false;

        ToggleCanvas(InitialMenuCanvas, true);
        ToggleCanvas(DialogueCanvas, false);
    }

    public override IEnumerator ZombieConversationAttempt()
    {
        yield return base.ZombieConversationAttempt();

        ToggleCanvas(InitialMenuCanvas, true);
    }
}
