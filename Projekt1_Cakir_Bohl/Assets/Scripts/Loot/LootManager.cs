using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LootManager : Manager
{
    [SerializeField] private Item[] _possibleItems;
    [SerializeField] private TextMeshProUGUI _textBox;

    private Item _item;
    private List<Item> _tempItemsAndAmounts = new List<Item>();

    void Start()
    {
        _textBox.enabled = true;
        _promptSkip.enabled = true;
        _promptContinue.enabled = false;

        foreach(Item item in _possibleItems)
        {
            _tempItemsAndAmounts.Add(item);
        }
        
        int lootCount = UnityEngine.Random.Range(1, GameConfig.LootableItems.Length);

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

        //yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);

        SceneManager.LoadScene(2);
    }
}
