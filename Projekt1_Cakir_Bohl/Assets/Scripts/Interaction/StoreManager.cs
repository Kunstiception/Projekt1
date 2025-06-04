using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class StoreManager : Manager, ISelectable
{
    [SerializeField] public Canvas MerchantInventoryCanvas;
    [SerializeField] public Canvas ItemToDoCanvas;
    [SerializeField] private TextMeshProUGUI _coinsText;
    private Item _currentItem;
    private Item _coinsItem;

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

    private void OnItemSelected(Item item, int index)
    {
        MerchantInventoryCanvas.GetComponent<Merchant>().IsActive = false;

        ToggleCanvas(ItemToDoCanvas, true);

        _currentItem = item;
    }

    private IEnumerator TryPurchase()
    {
        if (_coinsItem == null)
        {
            _currentLine = "You don't have any coins.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            ResetUI();

            yield break;
        }

        if (InventoryManager.Instance.InventoryItems.Count >= GameConfig.MaxInventorySlots)
        {
            _currentLine = "Your inventory is full.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            ResetUI();

            yield break;
        }

        int currentCoins = InventoryUtil.ReturnItemAmount(_coinsItem);

        if (currentCoins > 0)
        {
            if (currentCoins >= _currentItem.StorePrice)
            {
                InventoryManager.Instance.InventoryAmounts[InventoryManager.Instance.InventoryItems.IndexOf(_coinsItem)] = currentCoins - _currentItem.StorePrice;

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

            ResetUI();
        }
    }

    private void ResetUI()
    {
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
        _coinsItem = InventoryManager.Instance.InventoryItems.FirstOrDefault(item => item is Coin);

        if (_coinsItem == null)
        {
            _coinsText.text = "Coins: 0";

            return;
        }

        _coinsText.text = $"Coins: {InventoryUtil.ReturnItemAmount(_coinsItem)}";
    }
}
