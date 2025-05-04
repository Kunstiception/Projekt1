using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RestingManager : Manager, ISelectable
{
    [SerializeField] public Canvas SelectionMenuCanvas;
    [SerializeField] public Canvas InventoryCanvas;
    [SerializeField] public Canvas ItemToDoCanvas;
    [SerializeField] private TextMeshProUGUI _textBox;
     [SerializeField] private TextMeshProUGUI _playerUIHealth;
    [SerializeField] private TextMeshProUGUI _playerUIEgo;
    [SerializeField] private Slider _playerHealthBarBelow;
    [SerializeField] private Slider _playerEgoBarBelow;
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

        _playerUIHealth.text = $"{PlayerManager.Instance.HealthPoints}/{GameConfig.PlayerStartingHealth}";
        _playerUIEgo.text = $"{PlayerManager.Instance.EgoPoints}/{GameConfig.PlayerStartingEgo}";

        // Weiße Healthbar setzen
        _playerHealthBarBelow.value = (float)PlayerManager.Instance.HealthPoints / (float)GameConfig.PlayerStartingHealth;
        var childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(_playerHealthBarBelow.gameObject);
        childSlider.GetComponent<Slider>().value = (float)PlayerManager.Instance.HealthPoints / (float)GameConfig.PlayerStartingHealth;

        _playerEgoBarBelow.value = (float)PlayerManager.Instance.EgoPoints / (float)GameConfig.PlayerStartingEgo;
        childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(_playerEgoBarBelow.gameObject);
        childSlider.GetComponent<Slider>().value = (float)PlayerManager.Instance.EgoPoints / (float)GameConfig.PlayerStartingEgo;
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
                ToggleCanvas(InventoryCanvas, true);

                _textBox.enabled = true;

                SelectionMenuCanvas.GetComponent<SelectionMenu>().enabled = false;

                break;

            case 2:
                SetUpNextDay();

                SceneManager.LoadScene(2);
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
                _currentLine = _currentItem.Description;
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

        if (random >= 6)
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
            healthHeal = PlayerManager.Instance.HealthPoints < GameConfig.PlayerStartingHealth ?
                UnityEngine.Random.Range(1, GameConfig.PlayerStartingHealth - PlayerManager.Instance.HealthPoints) : 0;
            

            int egoHeal;
            egoHeal = PlayerManager.Instance.EgoPoints < GameConfig.PlayerStartingEgo ?
                UnityEngine.Random.Range(1, GameConfig.PlayerStartingEgo - PlayerManager.Instance.EgoPoints) : 0;


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

            SetUpNextDay();

            SceneManager.LoadScene(4);
            yield break;
        }
        else
        {
            //Wait for anim
            yield return new WaitForSeconds(5);

            StartCoroutine(UpdateUI(GameConfig.PlayerStartingHealth - PlayerManager.Instance.HealthPoints, true, PlayerManager.Instance.HealthPoints));
            StartCoroutine(UpdateUI(GameConfig.PlayerStartingEgo - PlayerManager.Instance.EgoPoints, false, PlayerManager.Instance.EgoPoints));
            
            PlayerManager.Instance.HealthPoints = GameConfig.PlayerStartingHealth;
            PlayerManager.Instance.EgoPoints = GameConfig.PlayerStartingEgo;
            
            _currentLine = "You have slept through the night and are now fully recovered!";
            yield return HandleTextOutput(_currentLine, false);
        }

        _textBox.enabled = false;
        ToggleCanvas(SelectionMenuCanvas, true);
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
    private IEnumerator UpdateUI(int healAmount, bool isHealthHeal, int initialAmount)
    {
        float healValue = 0;
        Slider slider = null;
        
        if (isHealthHeal)
        {
            slider = _playerHealthBarBelow;
            healValue = (float)healAmount / (float)GameConfig.PlayerStartingHealth;
            _playerUIHealth.text = $"{initialAmount + healAmount}/{GameConfig.PlayerStartingHealth}";
        }
        else
        {
            slider = _playerEgoBarBelow;
            healValue = (float)healAmount / (float)GameConfig.PlayerStartingEgo;
            _playerUIEgo.text = $"{initialAmount + healAmount}/{GameConfig.PlayerStartingEgo}";
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
    private void SetUpNextDay()
    {
        MainManager.Instance.CurrentDay++;
        MainManager.Instance.WayPoints.Clear();
        MainManager.Instance.WayPointTypes.Clear();
        MainManager.Instance.LastWayPoint = "";
    }
}
