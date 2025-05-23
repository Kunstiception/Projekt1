using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LootManager : Manager
{
    [SerializeField] private Item[] _possibleItems;

    private Item _item;
    private List<Item> _tempItemsAndAmounts = new List<Item>();

    IEnumerator Start()
    {
        _textBox.enabled = true;
        _promptSkip.enabled = true;
        _promptContinue.enabled = false;

        Canvas statsCanvas =  _playerHealthbarSection.GetComponentInParent<Canvas>();
        statsCanvas.enabled = false;

        foreach(Item item in _possibleItems)
        {
            _tempItemsAndAmounts.Add(item);
        }
        
        int lootCount = UnityEngine.Random.Range(1, GameConfig.MaximumLootableItems + 1);

        if(EvaluateVampire())
        {          
            InitializePlayerStats();

            _currentLine = UIDialogueStorage.VampireSunDamageLines[0]; 
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            statsCanvas.enabled = true;

            StartCoroutine(UpdateUI(GameConfig.VampireSunDamage, PlayerManager.Instance.HealthPoints));

            PlayerManager.Instance.HealthPoints -= GameConfig.VampireSunDamage;

            _currentLine = UIDialogueStorage.VampireSunDamageLines[1];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            statsCanvas.enabled = false;
        }

        StartCoroutine(SelectRandomItemsAndAmounts(lootCount));
    }

    void Update()
    {
        // Ermöglicht sofortiges Anzeigen der gesamten derzeitigen Line
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_textCoroutine != null)
            {
                _promptSkip.enabled = false;
                StopCoroutine(_textCoroutine);
                _textCoroutine = null;
                DialogueUtil.ShowFullLine(_currentLine, _textBox, _promptSkip);
            }
        }
    }

    private IEnumerator SelectRandomItemsAndAmounts(int lootCount)
    {     
        int randomIndex;
        int randomAmount;

        _currentLine = "There is a treasure chest!";
        yield return HandleTextOutput(_currentLine, false);

        for(int i = 1; i <= lootCount; i++)
        {
            randomIndex = UnityEngine.Random.Range(0, _tempItemsAndAmounts.Count);

            _item = _tempItemsAndAmounts[randomIndex];

            randomAmount = UnityEngine.Random.Range(_item.MinimumAmountOnLoot, _item.MaximumAmountOnLoot + 1);

            _tempItemsAndAmounts.RemoveAt(randomIndex);

            InventoryManager.Instance.ManageInventory(_item, randomAmount, true);

            _currentLine = DialogueUtil.AddEnding($"You have found {randomAmount} {_item.Name}" !, randomAmount);

            yield return HandleTextOutput(_currentLine, false);
        }

        foreach(Item key in InventoryManager.Instance.Inventory.Keys)
        {
            Debug.Log(key);
            Debug.Log(InventoryManager.Instance.Inventory[key]);
        } 

        yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);

        SceneManager.LoadScene(2);
    }

    private IEnumerator UpdateUI(int damage, int currentHealth)
    {
        float hitValue = 0;

        hitValue = (float)damage / (float)PlayerManager.Instance.GetStartingHealth();

        if(currentHealth <= 0)
        {
            currentHealth = 0;
            // GameOver Screen
        }

        _playerUIHealth.text = $"{currentHealth - damage}/{PlayerManager.Instance.GetStartingHealth()}";

        float currentValue = _playerHealthBarBelow.value;       
        float nextValue = currentValue - hitValue;
        float lerpValue = 0;

        // Weiße Healthbar setzen
        var childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(_playerHealthBarBelow.gameObject);
        childSlider.GetComponent<Slider>().value = nextValue;

        yield return new WaitForSeconds(GameConfig.TimeBeforeHealthbarUpdate);

        while (lerpValue <= 1 && lerpValue >= 0)
        {
            lerpValue += GameConfig.BarsLerpSpeed * Time.deltaTime;
            _playerHealthBarBelow.value = Mathf.Lerp(currentValue, nextValue, lerpValue / hitValue);
            yield return null;
        }

        _playerHealthBarBelow.value = nextValue;
    }
}
