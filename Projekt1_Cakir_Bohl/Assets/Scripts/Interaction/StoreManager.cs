using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StoreManager : Manager, ISelectable
{
    [SerializeField] public Canvas MerchantInventoryCanvas;
    [SerializeField] public Canvas ItemToDoCanvas;
    [SerializeField] private TextMeshProUGUI _coinsText;
    private Item _currentItem;
    private KeyValuePair<Item, int> _coinsKVP;

    void Start()
    {
        UpdateCoinsText();
    }

    private void OnEnable()
    {
        InventoryDisplayer.itemSelection += OnItemSelected;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        InventoryDisplayer.itemSelection -= OnItemSelected;
    }

    private void OnItemSelected(Item item)
    {
        MerchantInventoryCanvas.GetComponent<Merchant>().IsActive = false;

        ToggleCanvas(ItemToDoCanvas, true);

        _currentItem = item;
    }

    private IEnumerator TryPurchase()
    {
        if (_coinsKVP.Key != null)
        {
            int currentCoins = _coinsKVP.Value;

            if (currentCoins > 0)
            {
                if (currentCoins >= _currentItem.StorePrice)
                {
                    InventoryManager.Instance.Inventory[_coinsKVP.Key] = currentCoins - _currentItem.StorePrice;

                    InventoryManager.Instance.ManageInventory(_currentItem, 1, true);

                    UpdateCoinsText();

                    _currentLine = $"You have purchased {_currentItem.Name} for {_currentItem.StorePrice} coins.";
                    yield return StartCoroutine(HandleTextOutput(_currentLine, false));
                }
                else
                {
                    _currentLine = "You don't have enough coins.";
                    yield return StartCoroutine(HandleTextOutput(_currentLine, false));
                }
            }
        }
        else
        {
            _currentLine = "You don't have any coins.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        MerchantInventoryCanvas.GetComponent<Merchant>().ShowItemDescriptionAndSetPrompt(_currentItem);

        ToggleCanvas(ItemToDoCanvas, true);
    }

    public void HandleSelectedMenuPoint(int index)
    {
        switch (index)
        {
            case 0:
                StartCoroutine(TryPurchase());
                ToggleCanvas(ItemToDoCanvas, false);

                break;

            case 1:
                ToggleCanvas(ItemToDoCanvas, false);
                MerchantInventoryCanvas.GetComponent<SelectionMenu>().IsActive = true;

                break;
        }
    }

    private void UpdateCoinsText()
    {
        // https://learn.microsoft.com/de-de/dotnet/api/system.linq.enumerable.firstordefault?view=net-8.0
        _coinsKVP = InventoryManager.Instance.Inventory.FirstOrDefault(keyValuePair => keyValuePair.Key is Coin);
        _coinsText.text = $"Coins: {_coinsKVP.Value}";
    }
}
