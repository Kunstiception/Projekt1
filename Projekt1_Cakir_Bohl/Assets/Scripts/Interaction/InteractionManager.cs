using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public enum SceneType
{
    IsMerchant  = 0,
    IsDog  = 1,
    IsNPC = 2
}

public class InteractionManager : Manager, ISelectable
{
    public SceneType sceneType;

    [SerializeField] public Canvas MerchantInventoryCanvas;
    [SerializeField] public Canvas ItemToDoCanvas;
    [SerializeField] public Canvas InitialMenuCanvas;
    [SerializeField] public Canvas DialogueCanvas;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private GameObject _merchant;

    private IEnumerator Start()
    {
        ToggleCursorState(true);
        ToggleCanvas(MerchantInventoryCanvas, false);
        ToggleCanvas(ItemToDoCanvas, false);
        ToggleCanvas(InitialMenuCanvas, false);
        ToggleCanvas(DialogueCanvas, false);

        _merchant.SetActive(false);

        Canvas statsCanvas = _playerHealthbarSection.GetComponentInParent<Canvas>();
        statsCanvas.enabled = false;

        _textBox.enabled = true;
        _promptSkip.enabled = false;
        _promptContinue.enabled = false;

        TogglePlayerStatsPosition(true);

        yield return StartCoroutine(EvaluateVampire());

        SetScene();

        if (sceneType == SceneType.IsMerchant)
        {
            _currentLine = "Feel free to take a look at my merchandise, dear knight.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

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

    private void SetScene()
    {
        int randomIndex = Random.Range(0, 1);

        switch (randomIndex)
        {
            case 0:
                sceneType = SceneType.IsMerchant;

                _merchant.SetActive(true);

                break;

            case 1:
                sceneType = SceneType.IsDog;

                break;

            case 2:
                sceneType = SceneType.IsNPC;

                break;
        }
    }

    public void HandleSelectedMenuPoint(int index)
    {
        switch (index)
        {
            case 0:
                TogglePlayerStatsPosition(false);
                ToggleCanvas(MerchantInventoryCanvas, true);
                ToggleCanvas(InitialMenuCanvas, false);

                break;

            case 1:
                ToggleCanvas(DialogueCanvas, true);
                ToggleCanvas(InitialMenuCanvas, false);

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
}
