using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LootManager : Manager
{
    [SerializeField] private Item[] _possibleItems;
    [SerializeField] private Item[] _possibleEquipment;

    private Item _item;
    private List<Item> _tempItems = new List<Item>();

    IEnumerator Start()
    {
        _textBox.enabled = true;
        _promptSkip.enabled = true;
        _promptContinue.enabled = false;

        Canvas statsCanvas =  _playerHealthbarSection.GetComponentInParent<Canvas>();
        statsCanvas.enabled = false;

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

        if (InventoryManager.Instance.InventoryItems.Count >= GameConfig.MaxInventorySlots)
        {
            _currentLine = "Your inventory is already full.";
            yield return HandleTextOutput(_currentLine, false);

            SceneManager.LoadScene(2);

            yield break;
        }
        
        foreach (Item item in _possibleItems)
        {
            _tempItems.Add(item);
        }

        _currentLine = "There is a treasure chest!";
        yield return HandleTextOutput(_currentLine, false);

        StartCoroutine(SelectRandomItemsAndAmounts(CreateLootCount()));
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

    private int CreateLootCount()
    {
        int lootCount = 0;
        int maxLootCount = GameConfig.MaxInventorySlots - InventoryManager.Instance.InventoryItems.Count;

        if (maxLootCount >= GameConfig.MaximumLootableItems)
        {
            lootCount = UnityEngine.Random.Range(1, GameConfig.MaximumLootableItems + 1);
        }
        else
        {
            lootCount = UnityEngine.Random.Range(1, maxLootCount + 1);
        }

        return lootCount;
    }

    private IEnumerator SelectRandomItemsAndAmounts(int lootCount)
    {
        int randomIndex;
        int randomAmount;

        for (int i = 1; i <= lootCount; i++)
        {
            if (!MainManager.Instance.IsDay && i == lootCount)
            {
                for (int j = 0; j < GameConfig.EquipmentToAdd; j++)
                {
                    randomIndex = UnityEngine.Random.Range(0, _possibleEquipment.Length);

                    _tempItems.Add(_possibleEquipment[randomIndex]);
                }
            }

            randomIndex = UnityEngine.Random.Range(0, _tempItems.Count);

            _item = _tempItems[randomIndex];

            if (_item is Equipment)
            {
                randomAmount = 1;
            }
            else
            {
                randomAmount = UnityEngine.Random.Range(_item.MinimumAmountOnLoot, _item.MaximumAmountOnLoot + 1);         
            }

            _tempItems.RemoveAt(randomIndex);                       

            InventoryManager.Instance.ManageInventory(_item, randomAmount, true);

            _currentLine = DialogueUtil.AddEnding($"You have found {randomAmount} {_item.Name}"!, randomAmount);

            yield return HandleTextOutput(_currentLine, false);
        }

        foreach (Item item in InventoryManager.Instance.InventoryItems)
        {
            Debug.Log(item);
            Debug.Log(InventoryUtil.ReturnItemAmount(item));
        }

        yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);

        SceneManager.LoadScene(2);
    }

    private IEnumerator UpdateUI(int damage, int currentHealth)
    {
        float hitValue = (float)damage / (float)PlayerManager.Instance.GetStartingHealth();

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
