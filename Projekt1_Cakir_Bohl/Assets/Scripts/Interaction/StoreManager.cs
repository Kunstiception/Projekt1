using System.Collections;
using System.Linq;
using UnityEngine;

public class StoreManager : Manager, ISelectable
{
    [SerializeField] public Canvas MerchantInventoryCanvas;
    [SerializeField] public Canvas ItemToDoCanvas;
    private Item _currentItem;

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

        _textBox.text = "";

        ToggleCanvas(ItemToDoCanvas, true);

        _currentItem = item;
    }

    private IEnumerator TryPurchase()
    {
        // https://learn.microsoft.com/de-de/dotnet/api/system.linq.enumerable.firstordefault?view=net-8.0
        var keyValuePair = InventoryManager.Instance.Inventory.FirstOrDefault(keyValuePair => keyValuePair.Key is Coin);

        if (keyValuePair.Key != null)
        {
            int currentCoins = keyValuePair.Value;
            
            if (currentCoins > 0)
            {
                if (currentCoins >= _currentItem.StorePrice)
                {
                    InventoryManager.Instance.Inventory[keyValuePair.Key] = currentCoins - _currentItem.StorePrice;

                    InventoryManager.Instance.ManageInventory(_currentItem, 1, true);

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

        _textBox.text = "";

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
}
