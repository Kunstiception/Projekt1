using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LootManager : Manager
{
    [SerializeField] private Item[] _possibleItems;
    [SerializeField] private Item[] _possibleEquipment;
    [SerializeField] private AudioClip _onLoot1;
    [SerializeField] private AudioClip _onLoot2;

    private Item _item;

    IEnumerator Start()
    {
        _audioSource = GetComponent<AudioSource>();

        ToggleCursorState(true);

        _textBox.enabled = true;

        SetPrompts();

        InitializePlayerStats();

        yield return StartCoroutine(EvaluateVampire());

        if (InventoryManager.Instance.InventoryItems.Count >= GameConfig.MaxInventorySlots)
        {
            _currentLine = UIDialogueStorage.InventoryFull;
            yield return HandleTextOutput(_currentLine, false);

            SceneManager.LoadScene(2);

            yield break;
        }

        _currentLine = UIDialogueStorage.FoundLoot;
        yield return HandleTextOutput(_currentLine, false);

        StartCoroutine(SelectRandomItems(CreateLootCount()));
    }

    void Update()
    {
        ListenForSkipOrAuto();
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
            if (DiceUtil.D10() < GameConfig.EquipmentChance)
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

            if (i == 1)
            {
                _audioSource.PlayOneShot(_onLoot1);
            }
            else
            {
                _audioSource.PlayOneShot(_onLoot2); 
            }

            _currentLine = _currentLine = $"You have found one {_item.Name}!";
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
