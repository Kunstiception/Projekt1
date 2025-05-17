using System.Collections;
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
        Item coin = new Coin();
        int currentCoins;

        // https://stackoverflow.com/questions/2829873/how-can-i-detect-if-this-dictionary-key-exists-in-c
        if (InventoryManager.Instance.Inventory.TryGetValue(coin, out currentCoins))
        {
            if (currentCoins >= _currentItem.StorePrice)
            {
                InventoryManager.Instance.Inventory[coin] = currentCoins - _currentItem.StorePrice;

                InventoryManager.Instance.ManageInventory(_currentItem, 1, true);

                _currentLine = $"You have purchased {_currentItem.Name} for {_currentItem.StorePrice}";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
            else
            {
                _currentLine = "You don't have enough coins.";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
        }
        else
        {
            _currentLine = "You don't have any coins.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        _textBox.text = "";

        ToggleCanvas(ItemToDoCanvas, true);

        // var merchant = MerchantInventoryCanvas.GetComponent<Merchant>();

        // merchant.IsActive = true;
        // merchant.InitializeMenu();
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
