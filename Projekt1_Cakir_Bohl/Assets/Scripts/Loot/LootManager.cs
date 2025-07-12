using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LootManager : Manager
{
    [SerializeField] private Item[] _possibleItems;
    [SerializeField] private Item[] _possibleEquipment;

    private Item _item;

    IEnumerator Start()
    {
        ToggleCursorState(true);

        _textBox.enabled = true;

        SetPrompts();

        InitializePlayerStats();

        yield return StartCoroutine(EvaluateVampire());

        if (InventoryManager.Instance.InventoryItems.Count >= GameConfig.MaxInventorySlots)
        {
            _currentLine = "Your inventory is already full.";
            yield return HandleTextOutput(_currentLine, false);

            SceneManager.LoadScene(2);

            yield break;
        }

        _currentLine = "There is a treasure chest!";
        yield return HandleTextOutput(_currentLine, false);

        StartCoroutine(SelectRandomItems(CreateLootCount()));
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
        int lootCount;

        int maxLootCount = GameConfig.MaxInventorySlots - InventoryManager.Instance.InventoryItems.Count;

        if (MainManager.Instance.IsDay)
        {
            lootCount = GameConfig.LootCountDay;
        }
        else
        {
            if (DiceUtil.D10() > GameConfig.EquipmentChance)
            {
                lootCount = GameConfig.LootCountNight;
            }
            else
            {
                lootCount = GameConfig.LootCountDay;
            }
        }

        return lootCount;
    }

    private IEnumerator SelectRandomItems(int lootCount)
    {
        int randomIndex;

        for (int i = 1; i <= lootCount; i++)
        {
            if (!MainManager.Instance.IsDay && i == lootCount)
            {
                randomIndex = UnityEngine.Random.Range(0, _possibleEquipment.Length);

                _item = _possibleEquipment[randomIndex];
            }
            else
            {         
                randomIndex = UnityEngine.Random.Range(0, _possibleItems.Length);

                _item = _possibleItems[randomIndex];
            }

            InventoryManager.Instance.ManageInventory(_item, 1, true);  

            _currentLine = _currentLine = $"You have found {_item.Name}!";
            yield return HandleTextOutput(_currentLine, false);
        }

        foreach (Item item in InventoryManager.Instance.InventoryItems)
        {
            Debug.Log(item);
            Debug.Log(InventoryUtil.ReturnItemAmount(item));
        }

        yield return new WaitForSeconds(GameConfig.WaitTimeAfterLoot);

        SceneManager.LoadScene(2);
    }
}
