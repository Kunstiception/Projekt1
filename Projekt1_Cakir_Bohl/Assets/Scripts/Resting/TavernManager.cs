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
    [SerializeField] private GameObject _tavernOutside;
    [SerializeField] private GameObject _tavernInside;
    [SerializeField] private AudioClip _vampireBite;

    private Item _coinsItem;
    private Coroutine _vampireBiteCoroutine;
    private int _currentCoinAmount;
    private bool _hasChosenToBite;

    IEnumerator Start()
    {
        _mainEffectsAudioSource = GetComponent<AudioSource>();

        ToggleCursorState(true);

        InitializePlayerStats();

        _hasChosenToBite = false;

        SetPrompts();

        _tavernInside.SetActive(false);
        _tavernOutside.SetActive(true);

        ToggleCanvas(DialogueCanvas, false);
        ToggleCanvas(SleepingSelectionCanvas, false);
        ToggleCanvas(VampireSelectionCanvas, false);

        _currentLine = UIDialogueStorage.PassingTheGate;
        yield return HandleTextOutput(_currentLine, false);

        yield return CheckConditionsCoroutine();

        _currentLine = UIDialogueStorage.LetPassed;
        yield return HandleTextOutput(_currentLine, false);

        _currentLine = UIDialogueStorage.InfrontOfTavern;
        yield return HandleTextOutput(_currentLine, false);

        _tavernInside.SetActive(true);
        _tavernOutside.SetActive(false);

        yield return PrintMultipleLines(UIDialogueStorage.ReachedTavernLines);

        ToggleCanvas(DialogueCanvas, true);

        _dialogueManager.InitialOptions = _initialLines[MainManager.Instance.CurrentDay];

        _dialogueManager.StartDialogue();

        PlayerManager.Instance.IsTalking = true;

        MainManager.Instance.NumberOfVillagersMet++;
    }

    void Update()
    {
        if (PlayerManager.Instance.IsTalking)
        {
            return;
        }

        ListenForSkipOrAuto();
    }

    public override void OnEnable()
    {
        base.OnEnable();

        DialogueManager.OnDialogueFinished += DialogueEnded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        
        DialogueManager.OnDialogueFinished -= DialogueEnded;

        StopAllCoroutines();
    }

    // Überprüft ob der Player mit seinen Zuständen die Stadt betreten darf
    // Wenn nicht, dann wird die Reaktion der Wache zusammengesetzt
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

        _currentLine = "'It's a";

        foreach (string line in lines)
        {
            _currentLine += line;
        }

        _currentLine += ". Catch it!'";

        yield return HandleTextOutput(_currentLine, false, true);

        _textBox.enabled = false;

        PlayerManager.Instance.GotCaught = true;

        SceneManager.LoadScene(4);
    }

    // Setzt die Szene nach Ende des Dialogs fort
    private void DialogueEnded()
    {
        PlayerManager.Instance.IsTalking = false;

        ToggleCanvas(DialogueCanvas, false);

        StartCoroutine(AfterDialogueCoroutine());
    }

    // Shießt Dialog-Sequenz ab und führt zu Vampir-Sequenz hin wenn zutreffend
    // Oder zeigt Schlaf-Auswahlmenü an
    private IEnumerator AfterDialogueCoroutine()
    {
        yield return PrintMultipleLines(UIDialogueStorage.AfterTavernDialogue);

        if (ConditionManager.Instance.IsVampire)
        {
            yield return _vampireBiteCoroutine = StartCoroutine(VampireBiteCoroutine());
        }

        ShowSleepDecision();
    }

    // Wenn genügend Coins vorhanden, kann ein Zimmer für die Nacht gekauft werden
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
                    MainManager.Instance.HasRoom = false;

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
                    StartCoroutine(VampireNotBitingCoroutine());

                    break;

                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    // Zieht die nötigen Coins aus dem Inventar ab
    // Oder zeigt an, ob nicht genügend Geld vorhanden
    private IEnumerator PurchaseRoomCoroutine()
    {
        ToggleCanvas(SleepingSelectionCanvas, false);

        if (_currentCoinAmount >= GameConfig.RoomCost)
        {
            MainManager.Instance.HasRoom = true;

            InventoryManager.Instance.ManageInventory(_coinsItem, GameConfig.RoomCost, false);

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
    
    // Stoppt Vampir-Sequenz und führt zur Schlaf-Auswahl
    private IEnumerator VampireNotBitingCoroutine()
    {
        StopCoroutine(_vampireBiteCoroutine);
        _vampireBiteCoroutine = null;

        ToggleCanvas(VampireSelectionCanvas, false);

        yield return PrintMultipleLines(UIDialogueStorage.VampireNotBitingTodayLines);

        ToggleCanvas(VampireSelectionCanvas, false);
        ToggleCanvas(SleepingSelectionCanvas, true);
        ShowSleepDecision();
    }

    // Lässt den Playe als Vampir versuchen eine Person zu beißen
    // Wird automatisch ausgewürfelt und aus zufälligen Sätzen zusammengesetzt
    // Wenn erfolgreich: Gibt für einen Tag Vampir-Boost
    // Sonst triggert der Kampf mit der Wache
    private IEnumerator VampireBiteCoroutine()
    {
        ToggleCanvas(DialogueCanvas, false);

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

        if (DiceUtil.D10() <= GameConfig.VampireCaughtChance)
        {

            yield return AnticipationTextCoroutine(false);

            _currentLine = UIDialogueStorage.VampireCaughtLines[UnityEngine.Random.Range(0, UIDialogueStorage.VampireCaughtLines.Length)];
            yield return HandleTextOutput(_currentLine, false);

            _textBox.enabled = false;

            PlayerManager.Instance.GotCaught = true;

            SceneManager.LoadScene(4);

            yield break;
        }

        yield return AnticipationTextCoroutine(true);

        _mainEffectsAudioSource.PlayOneShot(_vampireBite, 1f);

        yield return PrintMultipleLines(UIDialogueStorage.VampireBiteLines);

        // Gewährt exra Boost für Vampir
        ConditionManager.Instance.ApplyVampireBiteBoost(true);

        MainManager.Instance.NumberOfPeopleBitten++;

        _textBox.text = "";

        InitializePlayerStats();

        ToggleCanvas(SleepingSelectionCanvas, true);
    }
}
