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
    private string[] _currentItemLines;
    private bool _isAmbush;
    private Item _currentItem;

    void Start()
    {
        _textBox.enabled = false;
        _promptContinue.enabled = false;
        _promptSkip.enabled = false;

        ToggleCanvas(InventoryCanvas, false);
        ToggleCanvas(ItemToDoCanvas, false);

        TogglePlayerStatsPosition(true);

        InitializePlayerStats();
    }

    private void OnEnable()
    {      
        InventoryDisplayer.itemSelection += SetCurrentItem;
        HealingItem.onHeal += SetUIUpdate;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        InventoryDisplayer.itemSelection -= SetCurrentItem;
        HealingItem.onHeal -= SetUIUpdate;
    }

    void Update()
    {
        ListenForSkip();
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
        switch(index)
        {
            case 0:
                _isAmbush = DecideIfAmbush();

                // Play corresponding animation

                StartCoroutine(SleepingCoroutine(_isAmbush));

                break;

            case 1:
                TogglePlayerStatsPosition(false);            
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
                StartCoroutine(UseOrDiscardItem(isUse: false));

                break;

            case 2:
                ToggleCanvas(ItemToDoCanvas, false);

                InventoryCanvas.GetComponent<InventoryDisplayer>().IsActive = true;
                _textBox.enabled = true;
                _textBox.text = _currentItem.Description;

                break;
            
            default:
                throw new IndexOutOfRangeException("Item or equipment could not be handled. Wrong index.");
        }
    }

    // Überprüft, welches Item ausgewählt wurde
    private IEnumerator CheckItemType()
    {   
        switch(_currentItem.ItemType)
        {
            case Item.ItemTypes.isUsable:
                StartCoroutine(UseOrDiscardItem(isUse: true));

                break;

            case Item.ItemTypes.isEquipment:
                // Methode zum Ausrüsten

                break;
            
            case Item.ItemTypes.isCurrency:
                _currentLine = _currentItem.LookAtText;
                yield return HandleTextOutput(_currentLine, false);

                ItemToDoCanvas.GetComponent<ItemToDoManager>().IsActive = true;

                break;
        }
    }

    // Verbraucht das Item (was eine Liste an strings zurückgibt, um die Aktion zu beschreiben) oder wirft es weg
    private IEnumerator UseOrDiscardItem(bool isUse)
    {
        if(!isUse)
        {
            _currentLine = $"You discard {_currentItem.Name}.";
            yield return HandleTextOutput(_currentLine, false);

            InventoryManager.Instance.ManageInventory(_currentItem, 1, false);

            InventoryCanvas.GetComponent<InventoryDisplayer>().UpdateDisplayedInventory(_currentItem);

            _textBox.text = "";

            if(InventoryManager.Instance.Inventory.Count > 0)
            {
                ItemToDoCanvas.GetComponent<ItemToDoManager>().IsActive = true;
            }
            else
            {
                ToggleCanvas(ItemToDoCanvas, false);
                ToggleCanvas(InventoryCanvas, true);
            }

            yield break;
        }

        _currentItemLines = _currentItem.GetComponent<IConsumable>().UseItem().ToArray();

        InventoryCanvas.GetComponent<InventoryDisplayer>().UpdateDisplayedInventory(_currentItem);

        _currentLine = _currentItemLines[0];
        yield return HandleTextOutput(_currentLine, false);

        if(_currentItemLines.Length > 1)
        {
            if(DiceUtil.D10() > 7)
            {
                _currentLine = _currentItemLines[UnityEngine.Random.Range(1, _currentItemLines.Length)];
                yield return HandleTextOutput(_currentLine, false);
            }
        }

        _textBox.text = "";

        if(InventoryManager.Instance.Inventory.Count > 0)
        {
            ItemToDoCanvas.GetComponent<ItemToDoManager>().IsActive = true;
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
        
        if(isAmbush)
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
            yield return new WaitForSeconds(5);

            if(healthHeal == 0 && egoHeal == 0)
            {
                _currentLine = $"You sleep well...";
                yield return HandleTextOutput(_currentLine, false);
            }
            else
            {
                StartCoroutine(UpdateUI(healthHeal, true, PlayerManager.Instance.HealthPoints - healthHeal));
                StartCoroutine(UpdateUI(egoHeal, false, PlayerManager.Instance.EgoPoints - egoHeal));

                _currentLine = $"You have recovered {healthHeal} health and {egoHeal} ego...";
                yield return HandleTextOutput(_currentLine, false);
            }

            _currentLine = "... before being ambushed!";
            yield return HandleTextOutput(_currentLine, false);
            
            PlayerManager.Instance.HasDisadvantage = true;

            SetUpNextDay(false);

            SceneManager.LoadScene(4);
            yield break;
        }
        else
        {
            //Wait for anim
            yield return new WaitForSeconds(5);

            bool wasHurt = false;

            if (PlayerManager.Instance.HealthPoints < PlayerManager.Instance.GetStartingHealth())
            {
                StartCoroutine(UpdateUI(PlayerManager.Instance.GetStartingHealth() - PlayerManager.Instance.HealthPoints, true, PlayerManager.Instance.HealthPoints));

                PlayerManager.Instance.HealthPoints = PlayerManager.Instance.GetStartingHealth();

                wasHurt = true;
            }

            if (PlayerManager.Instance.EgoPoints < PlayerManager.Instance.GetStartingEgo())
            {
                StartCoroutine(UpdateUI(PlayerManager.Instance.GetStartingEgo() - PlayerManager.Instance.EgoPoints, false, PlayerManager.Instance.EgoPoints));

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

            if(ConditionManager.Instance.IsSleepDeprived)
            {
                yield return StartCoroutine(PrintMultipleLines(ConditionManager.Instance.ApplyCondition(ConditionManager.Conditions.SleepDeprived, false)));
            }

            SetUpNextDay(true);

            if(ConditionManager.Instance.IsWerewolf)
            {
                ConditionManager.Instance.ToggleWerewolfStats(false);
                
                yield return StartCoroutine(PrintMultipleLines(UIDialogueStorage.WerewolfDayLines));
            }

            SceneManager.LoadScene(2);
            yield break;
        }
    }

    // Nach Auswahl im Menü wird hier das derzeitige Item gesetzt
    private void SetCurrentItem(Item item)
    {
        _currentItem = item;
    }

    // Nimmt die nötigen Infos für das UI-Update auf und startet Coroutine
    private void SetUIUpdate(bool isHealthHeal, int initialAmount, int healingAmount)
    {
        StartCoroutine(UpdateUI(healingAmount, isHealthHeal, initialAmount));
    }

    // Visualisiert Heilung von Health oder Ego in den Leisten
    private IEnumerator UpdateUI(int healAmount, bool isHealthChange, int initialAmount)
    {
        float healValue = 0;
        Slider slider = null;
        
        if (isHealthChange)
        {
            slider = _playerHealthBarBelow;
            healValue = (float)healAmount / (float)PlayerManager.Instance.GetStartingHealth();
            _playerUIHealth.text = $"{initialAmount + healAmount}/{PlayerManager.Instance.GetStartingHealth()}";
        }
        else
        {
            slider = _playerEgoBarBelow;
            healValue = (float)healAmount / (float)PlayerManager.Instance.GetStartingEgo();
            _playerUIEgo.text = $"{initialAmount + healAmount}/{PlayerManager.Instance.GetStartingEgo()}";
        }

        float currentValue = slider.value;       
        float nextValue = currentValue + healValue;
        float lerpValue = 0;

        // Untere Healthbar setzen
        slider.value = nextValue;

        yield return new WaitForSeconds(GameConfig.TimeBeforeHealthbarUpdate);

        var childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(slider.gameObject);
        childSlider.GetComponent<Slider>().value = nextValue;

        while (lerpValue <= 1 && lerpValue >= 0)
        {
            lerpValue += GameConfig.BarsLerpSpeed * Time.deltaTime;
            childSlider.value = Mathf.Lerp(currentValue, nextValue, lerpValue / healValue);
            yield return null;
        }

        childSlider.value = nextValue;
    }

     // Setzt die Variablen im MainManager zurück, damit diese mit den Daten des nächsten Tages befüllt werden können
    private void SetUpNextDay(bool isDay)
    {
        MainManager.Instance.CurrentDay++;
        MainManager.Instance.WayPoints.Clear();
        MainManager.Instance.WayPointTypes.Clear();
        MainManager.Instance.LastWayPoint = "";
        MainManager.Instance.IsDay = isDay;
    }

    private IEnumerator EndWithoutSleep()
    {
        _textBox.enabled = true;

        if(!ConditionManager.Instance.IsSleepDeprived)
        {
            PlayerManager.Instance.LatestCondition = ConditionManager.Conditions.SleepDeprived;
            
            yield return StartCoroutine(PrintMultipleLines(ConditionManager.Instance.ApplyCondition(ConditionManager.Conditions.SleepDeprived, true)));
        }

        SetUpNextDay(false);

        if(ConditionManager.Instance.IsWerewolf)
        {
            ConditionManager.Instance.ToggleWerewolfStats(true);
            
            yield return StartCoroutine(PrintMultipleLines(UIDialogueStorage.WerewolfNightLines));
        }

        SceneManager.LoadScene(8);
    }
}
