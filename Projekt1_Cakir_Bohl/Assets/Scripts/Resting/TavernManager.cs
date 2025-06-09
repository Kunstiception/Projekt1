using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernManager : Manager
{
    [SerializeField] public Canvas InitialMenuCanvas;
    [SerializeField] public Canvas DialogueCanvas;
    [SerializeField] private DialogueLines[] _initialLines;
    [SerializeField] private DialogueManager _dialogueManager;

    IEnumerator Start()
    {
        _promptContinue.enabled = false;

        ToggleCanvas(DialogueCanvas, false);
        ToggleCanvas(InitialMenuCanvas, false);

        yield return PrintMultipleLines(UIDialogueStorage.ReachingTavernLines);

        ToggleCanvas(DialogueCanvas, true);

        _dialogueManager.InitialOptions = _initialLines[MainManager.Instance.CurrentDay];

        _dialogueManager.StartDialogue();
    }

    void Update()
    {
        ListenForSkip();
    }

    private void OnEnable()
    {
        DialogueManager.onDialogueFinished += ShowSleepDecision;
    }

    private void OnDisable()
    {
        DialogueManager.onDialogueFinished -= ShowSleepDecision;

        StopAllCoroutines();
    }

    private void ShowSleepDecision()
    {
    ToggleCanvas(DialogueCanvas, false);
        ToggleCanvas(InitialMenuCanvas, true);
    }

}
