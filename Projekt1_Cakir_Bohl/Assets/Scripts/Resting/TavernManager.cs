using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TavernManager : Manager, ISelectable
{
    [SerializeField] public Canvas SleepingSelectionCanvas;
    [SerializeField] public Canvas VampireSelectionCanvas;
    [SerializeField] public Canvas DialogueCanvas;
    [SerializeField] private DialogueLines[] _initialLines;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private TextMeshProUGUI _roomText;
    [SerializeField] private TextMeshProUGUI _coinsText;
    private Item _coinsItem;
    private Coroutine _vampireBiteCoroutine;
    private int _currentCoinAmount;
    private bool _hasChosenToBite;

    IEnumerator Start()
    {
        _hasChosenToBite = false;

        _promptContinue.enabled = false;

        ToggleCanvas(DialogueCanvas, false);
        ToggleCanvas(SleepingSelectionCanvas, false);
        ToggleCanvas(VampireSelectionCanvas, false);

        _currentLine = "As you pass the gate the watchful eyes of a guard set upon you.";
        yield return HandleTextOutput(_currentLine, false);

        yield return CheckConditionsCoroutine();

        _currentLine = "He lets you enter.";
        yield return HandleTextOutput(_currentLine, false);

        yield return PrintMultipleLines(UIDialogueStorage.ReachingTavernLines);

        ToggleCanvas(DialogueCanvas, true);

        //_dialogueManager.InitialOptions = _initialLines[MainManager.Instance.CurrentDay];
        _dialogueManager.InitialOptions = _initialLines[0];

        _dialogueManager.StartDialogue();
    }

    void Update()
    {
        ListenForSkip();
    }

    private void OnEnable()
    {
        DialogueManager.onDialogueFinished += DialogueEnded;
    }

    private void OnDisable()
    {
        DialogueManager.onDialogueFinished -= DialogueEnded;

        StopAllCoroutines();
    }

    private IEnumerator CheckConditionsCoroutine()
    {
        var conditions = ConditionManager.Instance.GetCurrentConditions();

        if (conditions.Count == 0)
        {
            yield break;
        }

        if (conditions.Count == 1 && conditions[0] == ConditionManager.Conditions.SleepDeprived)
        {
            _currentLine = "You can barely keep your eyes open.";
            yield return HandleTextOutput(_currentLine, false);

            yield break;
        }

        List<string> lines = new List<string>();

        foreach (ConditionManager.Conditions condition in conditions)
        {
            string conditionName = null;

            if (condition == ConditionManager.Conditions.Werewolf && !MainManager.Instance.IsDay)
            {
                conditionName = "Werewolf";
            }

            if (condition == ConditionManager.Conditions.Zombie)
            {
                conditionName = "Zombie";
            }

            if (conditionName == null)
            {
                continue;
            }

            if (lines.Count == 0)
            {
                lines.Add($" {conditionName}");

                continue;
            }

            lines.Add($" -{conditionName}");
        }

        if (lines.Count == 0)
        {
            yield break;
        }

        yield return PrintMultipleLines(UIDialogueStorage.GettingCaughtAtTheGateLines);

        _currentLine = "It's a";

        foreach (string line in lines)
        {
            _currentLine += line;
        }

        _currentLine += ". Catch it!";

        yield return HandleTextOutput(_currentLine, false);

        _textBox.enabled = false;

        PlayerManager.Instance.GotCaught = true;

        SceneManager.LoadScene(4);
    }

    private void DialogueEnded()
    {
        if (ConditionManager.Instance.IsVampire)
        {
            _vampireBiteCoroutine = StartCoroutine(VampireBiteCoroutine());

            return;
        }

        ShowSleepDecision();
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
            _coinsText.text = $"Coins: {_currentCoinAmount}";
        }

        ToggleCanvas(DialogueCanvas, false);
        ToggleCanvas(SleepingSelectionCanvas, true);
    }

    public void HandleSelectedMenuPoint(int index)
    {
        if (SleepingSelectionCanvas.isActiveAndEnabled)
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
        else
        {
            switch (index)
            {
                case 0:
                    _hasChosenToBite = true;

                    break;

                case 1:
                    StopCoroutine(_vampireBiteCoroutine);

                    _vampireBiteCoroutine = null;
                    
                    ToggleCanvas(VampireSelectionCanvas, false);
                    ToggleCanvas(SleepingSelectionCanvas, true);

                    break;

                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    private IEnumerator PurchaseRoomCoroutine()
    {
        ToggleCanvas(SleepingSelectionCanvas, false);

        if (_currentCoinAmount >= GameConfig.RoomCost)
        {
            PlayerManager.Instance.HasRoom = true;

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

    private IEnumerator VampireBiteCoroutine()
    {
        yield return PrintMultipleLines(UIDialogueStorage.VampireInTheCityLines);

        _textBox.text = "";

        ToggleCanvas(VampireSelectionCanvas, true);

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => _hasChosenToBite == true);

        ToggleCanvas(VampireSelectionCanvas, false);

        _currentLine = "You start looking for your victim in the tavern.";
        yield return HandleTextOutput(_currentLine, false);

        _currentLine = UIDialogueStorage.VampireLookingForVictimLines[UnityEngine.Random.Range(0, UIDialogueStorage.VampireLookingForVictimLines.Length)];
        yield return HandleTextOutput(_currentLine, false);

        _textBox.text = "";

        yield return AnticipationTextCoroutine();

        if (DiceUtil.D10() <= GameConfig.VampireCaughtChance)
        {
            _currentLine = UIDialogueStorage.VampireCaughtLines[UnityEngine.Random.Range(0, UIDialogueStorage.VampireCaughtLines.Length)];
            yield return HandleTextOutput(_currentLine, false);

            _textBox.enabled = false;

            PlayerManager.Instance.GotCaught = true;

            SceneManager.LoadScene(4);

            yield break;
        }

        yield return PrintMultipleLines(UIDialogueStorage.VampireBiteLines);

        // Gewährt exra Boost für Vampir
        ConditionManager.Instance.ApplyVampireBiteBoost(true);

        _textBox.text = "";

        ToggleCanvas(SleepingSelectionCanvas, true);
    }
}
