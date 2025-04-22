using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor;

public class LootManager : MonoBehaviour
{
    [SerializeField] private Item[] _possibleItems;
    [SerializeField] private TextMeshProUGUI _promptSkip;
    [SerializeField] private TextMeshProUGUI _promptContinue;
    [SerializeField] private TextMeshProUGUI _textBox;
    [SerializeField] private SceneAsset _nextScene;

    private Item _item;
    private string _currentLine;
    private Coroutine _textCoroutine;
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
        int randomIndex = 0;
        int randomAmount = 0;
        bool isLastItem = false;

        _currentLine = "There is a treasure chest!";
        yield return HandleTextOutput(_currentLine, false);

        for(int i = 1; i <= lootCount; i++)
        {
            if((lootCount - i) == 1)
            {
                isLastItem = true;
            }

            randomIndex = UnityEngine.Random.Range(0, _tempItemsAndAmounts.Count);

            _item = _tempItemsAndAmounts[randomIndex];

            randomAmount = UnityEngine.Random.Range(_item.MinimumAmountOnLoot, _item.MaximumAmountOnLoot + 1);

            _tempItemsAndAmounts.RemoveAt(randomIndex);

            PlayerManager.Instance.ManageInventory(_item, randomAmount, true);

            _currentLine = AddEnding($"You have found {randomAmount} {_item.Name}", randomAmount);

            yield return HandleTextOutput(_currentLine, isLastItem);
        }

        foreach(string key in PlayerManager.Instance.Inventory.Keys)
        {
            Debug.Log(key);
            Debug.Log(PlayerManager.Instance.Inventory[key]);
        } 

        yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);

        SceneManager.LoadScene(_nextScene.name);
    }

    private string AddEnding(string line, int count)
    {
        if(count > 1)
        {
            return line + "s!";
        }
         return line + "!";
    }

    private IEnumerator HandleTextOutput(string line, bool isLastLine)
    {
        _textBox.enabled = true;
        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _textBox, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => _currentLine == _textBox.text);

        if (isLastLine)
        {          
            yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);
            _textBox.enabled = false;
            yield break;
        }

        // Einen Frame warten, damit Input nicht beide GetKeyDown-Events triggert
        yield return null;

        yield return StartCoroutine(DialogueUtil.WaitForContinue(_promptContinue));

        _textBox.enabled = false;
    }
}
