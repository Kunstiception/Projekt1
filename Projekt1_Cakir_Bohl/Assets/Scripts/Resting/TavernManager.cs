using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TavernManager : Manager, ISelectable
{
    [SerializeField] public Canvas SleepingSelectionCanvas;
    [SerializeField] public Canvas DialogueCanvas;
    [SerializeField] private DialogueLines[] _initialLines;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private TextMeshProUGUI _roomText;
    [SerializeField] private TextMeshProUGUI _coinsText;
    private Item _coinsItem;
    private int _currentCoinAmount;

    IEnumerator Start()
    {
        _promptContinue.enabled = false;

        ToggleCanvas(DialogueCanvas, false);
        ToggleCanvas(SleepingSelectionCanvas, false);

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
        _roomText.text = $"Take a room in the inn. (costs {GameConfig.RoomCost} G)";

        _coinsItem = InventoryUtil.ReturnCoinItem();

        _currentCoinAmount = InventoryUtil.ReturnItemAmount(_coinsItem);

        if (_coinsItem == null)
        {
            _coinsText.text = "Coins: 0";
        }
        else
        {
            _coinsText.text = $"{_currentCoinAmount}";
        }

        ToggleCanvas(DialogueCanvas, false);
        ToggleCanvas(SleepingSelectionCanvas, true);
    }

    public void HandleSelectedMenuPoint(int index)
    {
        switch (index)
        {
            case 0:
                StartCoroutine(PurchaseRoomCoroutine());

                break;

            case 1:
                PlayerManager.Instance.HasRoom = false;
                
                SceneManager.LoadScene(7);

                break;

            default:
                throw new IndexOutOfRangeException();

        }
    }

    private IEnumerator PurchaseRoomCoroutine()
    {
        ToggleCanvas(SleepingSelectionCanvas, false);

        if (_currentCoinAmount >= GameConfig.RoomCost)
        {
            InventoryManager.Instance.InventoryAmounts[InventoryManager.Instance.InventoryItems.IndexOf(_coinsItem)] = _currentCoinAmount - GameConfig.RoomCost;

            _currentLine = $"You are led to a warm and cozy room on the first floor of the inn.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            SceneManager.LoadScene(7);
        }
        else
        {
            _currentLine = "You don't have enough coins.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            _textBox.text = "";

            ToggleCanvas(SleepingSelectionCanvas, true);
        }
    }
}
