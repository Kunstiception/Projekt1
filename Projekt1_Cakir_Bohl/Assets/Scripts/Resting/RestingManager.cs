using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RestingManager : Manager, ISelectable, ICondition
{
    [SerializeField] public Canvas SelectionMenuCanvas;
    [SerializeField] public Canvas InventoryCanvas;
    [SerializeField] public Canvas ItemToDoCanvas;
    [SerializeField] private GameObject _outsideBackground;
    [SerializeField] private GameObject _roomBackground;
    [SerializeField] private AudioClip _atmoOutside;
    [SerializeField] private AudioClip _atmoInside;
    [SerializeField] private AudioSource _atmoSource;
    [SerializeField] private TextMeshProUGUI _continueText;
    private InventoryDisplayer _inventoryDisplayer;
    private bool _isAmbush;
    private int _currentIndex;

    void Start()
    {
        ToggleCursorState(true);

        _textBox.enabled = false;
        _promptContinue.enabled = false;
        _promptSkip.enabled = false;

        _inventoryDisplayer = InventoryCanvas.GetComponent<InventoryDisplayer>();

        if (MainManager.Instance.IsDay)
        {
            _continueText.text = "Venture into the night";
        }
        else
        {
            _continueText.text = "Stay up till morning";
        }

        if (PlayerManager.Instance.HasRoom)
        {
            _roomBackground.SetActive(true);
            _outsideBackground.SetActive(false);
            _atmoSource.clip = _atmoInside;
        }
        else
        {
            _roomBackground.SetActive(false);
            _outsideBackground.SetActive(true);
            _atmoSource.clip = _atmoOutside;
        }

        _atmoSource.Play();

        ToggleCanvas(InventoryCanvas, false);
        ToggleCanvas(ItemToDoCanvas, false);

        InitializePlayerStats();
    }

    private void OnEnable()
    {
        InventoryDisplayer.itemSelection += SetCurrentItemAndIndex;
        HealingItem.onHeal += SetUIUpdate;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        InventoryDisplayer.itemSelection -= SetCurrentItemAndIndex;
        HealingItem.onHeal -= SetUIUpdate;
    }

    void Update()
    {
        ListenForSkipOrAuto();
    }

    // Bestimmt, was die Auswahl im Menü auslöst
    public void HandleSelectedMenuPoint(int index)
    {
        if (SelectionMenuCanvas.isActiveAndEnabled == false)
        {
            HandleSelectedItemOrEquipment(index);

            return;
        }

        ToggleCanvas(SelectionMenuCanvas, false);

        // 0 = sleep, 1 = show items, 2 = continue quest
        switch (index)
        {
            case 0:
                _isAmbush = DecideIfAmbush();

                StartCoroutine(SleepingCoroutine(_isAmbush));

                break;

            case 1:
                ToggleCanvas(InventoryCanvas, true);

                _textBox.enabled = true;

                SelectionMenuCanvas.GetComponent<SelectionMenu>().enabled = false;

                break;

            case 2:
                StartCoroutine(EndWithoutSleep());

                break;
        }
    }

    // Anhängig davon, welche Art von Item ausgewählt wurde, wird eine Aktion ausgeführt
    public void HandleSelectedItemOrEquipment(int index)
    {
        switch (index)
        {
            case 0:
                StartCoroutine(CheckItemType());

                break;

            case 1:
                StartCoroutine(DiscardItem());

                break;

            case 2:
                ToggleCanvas(ItemToDoCanvas, false);

                _inventoryDisplayer.IsActive = true;
                _textBox.enabled = true;
                _textBox.text = _currentItem.Description;

                break;

            default:
                throw new IndexOutOfRangeException("Item or equipment could not be handled. Wrong index.");
        }
    }

    private IEnumerator DiscardItem()
    {
        ToggleCanvas(ItemToDoCanvas, false);

        _currentLine = $"You discard {_currentItem.Name}.";
        yield return HandleTextOutput(_currentLine, false);

        if (_currentItem is Equipment && InventoryManager.Instance.CheckIfEquipped(_currentItem))
        {
            var iEquipable = _currentItem as IEquipable;

            iEquipable.EquipItem(false);       
        }

        InventoryManager.Instance.ManageInventory(_currentItem, 1, false, _currentIndex);

        _inventoryDisplayer.UpdateDisplayedInventory(_currentItem);

        _inventoryDisplayer.UpdateEquipIndicators();

        InitializePlayerStats();

        ToggleCanvas(InventoryCanvas, true);
    }

    // Überprüft, welches Item ausgewählt wurde
    private IEnumerator CheckItemType()
    {
        ToggleCanvas(ItemToDoCanvas, false);

        switch (_currentItem.ItemType)
        {
            case Item.ItemTypes.isUsable:
                StartCoroutine(UseSelectedItem());

                break;

            case Item.ItemTypes.isEquipment:
                if (_currentItem is not Equipment)
                {
                    break;
                }

                if (InventoryUtil.CheckIfEquipable(_currentIndex))
                {
                    StartCoroutine(EquipSelectedItem(true));

                    break;
                }

                StartCoroutine(EquipSelectedItem(false));

                break;

            case Item.ItemTypes.isCurrency:
                if (_currentItem is not Coin)
                {
                    break;
                }

                Coin coin = (Coin)_currentItem;

                _currentLine = coin.LookAtText;
                yield return HandleTextOutput(_currentLine, false);

                _inventoryDisplayer.ShowItemDescriptionAndSetPrompt(_currentItem);

                ToggleCanvas(ItemToDoCanvas, true);

                break;
        }
    }

    // Verbraucht das Item (was eine Liste an strings zurückgibt, um die Aktion zu beschreiben)
    public override IEnumerator UseSelectedItem()
    {
        ToggleCanvas(ItemToDoCanvas, false);

        yield return base.UseSelectedItem();

        Item previousItem = _currentItem;

        _inventoryDisplayer.ShowItemDescriptionAndSetPrompt(_currentItem);
        _inventoryDisplayer.UpdateDisplayedInventory(_currentItem);

        if (!InventoryManager.Instance.InventoryItems.Contains(previousItem) || InventoryManager.Instance.InventoryItems.Count <= 0)
        {
            ToggleCanvas(InventoryCanvas, true);
        }
        else
        {
            ToggleCanvas(ItemToDoCanvas, true);
        }
    }

    // Equipment anlegen
    private IEnumerator EquipSelectedItem(bool isEquip)
    {
        if (_currentItem is not IEquipable)
        {
            yield break;
        }

        var iEquipable = _currentItem as IEquipable;

        if (isEquip)
        {
            if (!InventoryManager.Instance.ManageEquipment(_currentItem, true, _currentIndex))
            {
                _currentLine = "Cannot equip item!";
                yield return HandleTextOutput(_currentLine, false);
            }
            else
            {

                yield return PrintMultipleLines(iEquipable.EquipItem(true).ToArray());
            }

        }
        else
        {
            InventoryManager.Instance.ManageEquipment(_currentItem, false, _currentIndex);

            yield return PrintMultipleLines(iEquipable.EquipItem(false).ToArray());
        }

        _inventoryDisplayer.UpdateEquipIndicators();

        _inventoryDisplayer.ShowItemDescriptionAndSetPrompt(_currentItem);

        InitializePlayerStats();

        if (InventoryManager.Instance.InventoryItems.Count > 0)
        {
            ToggleCanvas(ItemToDoCanvas, true);
        }
        else
        {
            ToggleCanvas(ItemToDoCanvas, false);
            ToggleCanvas(InventoryCanvas, true);
        }
    }

    // Hier wird festegelegt, ob der Player in der Nacht in einen Kampf gezwungen wird
    private bool DecideIfAmbush()
    {
        if (PlayerManager.Instance.HasRoom)
        {
            return false;
        }

        int random = DiceUtil.D10();

        if (random <= GameConfig.AmbushChance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Händelt die im Schlaf erlangte Regeneration (volle Regeneration wenn kein Ambush, teilweise Regeneration bei Ambush)
    private IEnumerator SleepingCoroutine(bool isAmbush)
    {
        _textBox.enabled = true;

        _currentLine = "You are falling asleep.";
        yield return HandleTextOutput(_currentLine, false);

        _textBox.text = "";

        if (isAmbush)
        {
            int healthHeal;
            healthHeal = PlayerManager.Instance.HealthPoints < PlayerManager.Instance.GetStartingHealth() ?
                UnityEngine.Random.Range(1, PlayerManager.Instance.GetStartingHealth() - PlayerManager.Instance.HealthPoints) : 0;


            int egoHeal;
            egoHeal = PlayerManager.Instance.EgoPoints < PlayerManager.Instance.GetStartingHealth() ?
                UnityEngine.Random.Range(1, PlayerManager.Instance.GetStartingEgo() - PlayerManager.Instance.EgoPoints) : 0;


            PlayerManager.Instance.HealthPoints += healthHeal;
            PlayerManager.Instance.EgoPoints += egoHeal;

            //Wait for anim
            yield return AnticipationTextCoroutine(false);

            if (healthHeal == 0 && egoHeal == 0)
            {
                _currentLine = $"You sleep well...";
                yield return HandleTextOutput(_currentLine, false);
            }
            else
            {
                StartCoroutine(UpdateUIHeal(healthHeal, true, PlayerManager.Instance.HealthPoints - healthHeal));
                StartCoroutine(UpdateUIHeal(egoHeal, false, PlayerManager.Instance.EgoPoints - egoHeal));

                _currentLine = $"You have recovered {healthHeal} health and {egoHeal} ego...";
                yield return HandleTextOutput(_currentLine, false);
            }

            _currentLine = "... before being ambushed!";
            yield return HandleTextOutput(_currentLine, false);

            if (ConditionManager.Instance.IsWerewolf && MainManager.Instance.IsDay)
            {
                yield return EvaluateWerewolfCondition(true);
            }

            if (DiceUtil.D10() <= GameConfig.DogSaveChance)
            {
                yield return PrintMultipleLines(UIDialogueStorage.DogSaveLines);

                SetUpNextDay(false);

                if (!ConditionManager.Instance.GetCurrentConditions().Contains(ConditionManager.Conditions.SleepDeprived))
                {
                    PlayerManager.Instance.LatestCondition = ConditionManager.Conditions.SleepDeprived;               
                }

                yield return StartCoroutine(PrintMultipleLines(ConditionManager.Instance.ApplyCondition(ConditionManager.Conditions.SleepDeprived, true)));

                SceneManager.LoadScene(8);
            }
            else
            {
                PlayerManager.Instance.HasDisadvantage = true;

                SetUpNextDay(false);

                SceneManager.LoadScene(4);
            }

            yield break;
        }
        else
        {
            MainManager.Instance.NumberOfNightsSlept++;
            
            yield return AnticipationTextCoroutine(true);

            bool wasHurt = false;

            if (PlayerManager.Instance.HealthPoints < PlayerManager.Instance.GetStartingHealth())
            {
                StartCoroutine(UpdateUIHeal(PlayerManager.Instance.GetStartingHealth() - PlayerManager.Instance.HealthPoints, true, PlayerManager.Instance.HealthPoints));

                PlayerManager.Instance.HealthPoints = PlayerManager.Instance.GetStartingHealth();

                wasHurt = true;
            }

            if (PlayerManager.Instance.EgoPoints < PlayerManager.Instance.GetStartingEgo())
            {
                StartCoroutine(UpdateUIHeal(PlayerManager.Instance.GetStartingEgo() - PlayerManager.Instance.EgoPoints, false, PlayerManager.Instance.EgoPoints));

                PlayerManager.Instance.EgoPoints = PlayerManager.Instance.GetStartingEgo();

                wasHurt = true;
            }

            if (wasHurt)
            {
                _currentLine = "You have slept through the night and are now fully recovered!";
            }
            else
            {
                _currentLine = "You have slept throught the night.";
            }

            yield return HandleTextOutput(_currentLine, false);

            if (ConditionManager.Instance.IsSleepDeprived)
            {
                yield return StartCoroutine(PrintMultipleLines(ConditionManager.Instance.ApplyCondition(ConditionManager.Conditions.SleepDeprived, false)));
            }

            SetUpNextDay(true);

            if (ConditionManager.Instance.IsWerewolf)
            {
                yield return EvaluateWerewolfCondition(false);
            }

            SceneManager.LoadScene(1);
            yield break;
        }
    }

    // Nach Auswahl im Menü wird hier das derzeitige Item gesetzt
    private void SetCurrentItemAndIndex(Item item, int index)
    {
        _currentItem = item;
        _currentIndex = index;
    }

    // Nimmt die nötigen Infos für das UI-Update auf und startet Coroutine
    private void SetUIUpdate(bool isHealthHeal, int initialAmount, int healingAmount)
    {
        StartCoroutine(UpdateUIHeal(healingAmount, isHealthHeal, initialAmount));
    }

    private IEnumerator EndWithoutSleep()
    {
        bool becameSleepDeprived = false;

        _textBox.enabled = true;

        if (!ConditionManager.Instance.IsSleepDeprived)
        {
            PlayerManager.Instance.LatestCondition = ConditionManager.Conditions.SleepDeprived;

            yield return StartCoroutine(PrintMultipleLines(ConditionManager.Instance.ApplyCondition(ConditionManager.Conditions.SleepDeprived, true)));

            becameSleepDeprived = true;
        }

        if (MainManager.Instance.IsDay)
        {
            SetUpNextDay(false);
            if (ConditionManager.Instance.IsWerewolf)
            {
                yield return EvaluateWerewolfCondition(true);
            }
        }
        else
        {
            SetUpNextDay(true);
            if (ConditionManager.Instance.IsWerewolf)
            {
                yield return EvaluateWerewolfCondition(false);
            }         
        }

        PlayerManager.Instance.HasFinishedDay = true;
        
        _textBox.enabled = false;

        if (becameSleepDeprived)
        {
            SceneManager.LoadScene(8);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }
}
