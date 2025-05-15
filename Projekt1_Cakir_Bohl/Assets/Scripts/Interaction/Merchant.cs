using System.Linq;
using TMPro;
using UnityEngine;

public class Merchant : InventoryDisplayer
{
    [SerializeField] private Item[] _availabeItems;
    [SerializeField] private TextMeshProUGUI[] _priceTexts;
    [SerializeField] private InteractionManager _interactionManager;
    private int _price;

    void Start()
    {
        IsActive = true;
        
        InitializeInventory();

        _textBox.text = _availabeItems.ElementAt(_currentMenuPoint).Description;

        SetInitialPointer();
    }

    void Update()
    {
        ListenForInputs();
    }

    public override void InitializeInventory()
    {
        for(int i = 0; i < _availabeItems.Length; i++)
        {
            _name = _availabeItems.ElementAt(i).Name;
            _price = _availabeItems.ElementAt(i).StorePrice;

            _menuPoints[i].text = _name;
            _priceTexts[i].text = $"{_price} G";
        }

        _menuPoints[_availabeItems.Length].text = "Return";
        _priceTexts[_availabeItems.Length].text = "";

        for(int i = _availabeItems.Length + 1; i < _menuPoints.Length; i++)
        {
            _menuPoints[i].text = "";
            _priceTexts[i].text = "";
            _pointers[i].enabled = false;
        }
    }

    public override void UpdateDisplayedInventory(Item item)
    {            
        if(_availabeItems.Length <= 0)
        {
            _menuPoints[0].text = "Return";
            _priceTexts[0].text = "";
        }
        
        if(InventoryManager.Instance.Inventory.ContainsKey(item))
        {
            _price = _availabeItems[_currentMenuPoint].StorePrice;
            _priceTexts[_currentMenuPoint].text = $"{_price} G";
        }
        else
        {
            for(int i = _currentMenuPoint; i < _menuPoints.Length; i++)
            {
                if(_menuPoints[i].text == "")
                {
                    break;
                }

                _menuPoints[i].text = _menuPoints[i + 1].text;
                _priceTexts[i].text = _priceTexts[i + 1].text;
            }
        }
    }

    public override void InitializeMenu()
    {
        base.InitializeMenu();

        if(_availabeItems.Length <= 0)
        {
            Debug.LogError("No items in the merchant's inventory. Please add items to the inventory.");
            return;
        }
        
        _textBox.text = _availabeItems[_currentMenuPoint].Description;
    }

    public override void ListenForInputs()
    {
        while(!IsActive)
        {
            return;
        }
        
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(_currentMenuPoint == _menuPoints.Length - 1)
            {
                return;
            }

            ChangePosition(isUp: false);
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
            if (_currentMenuPoint == 0)
            {
                return;
            }

            ChangePosition(isUp: true);
        }
        else if(Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            if(_currentMenuPoint == _availabeItems.Length || _availabeItems.Length == 0)
            {
                // _restingManager.ToggleCanvas(_restingManager.SelectionMenuCanvas, true);
                // _restingManager.ToggleCanvas(_restingManager.InventoryCanvas, false);
                // _restingManager.TogglePlayerStatsPosition(true);

                _textBox.text = "";

                SetInitialPointer();

                return;
            }

            // _restingManager.ToggleCanvas(_restingManager.ItemToDoCanvas, true);
            // _restingManager.ToggleCanvas(_restingManager.SelectionMenuCanvas, false);

            //itemSelection?.Invoke(InventoryManager.Instance.Inventory.ElementAt(_currentMenuPoint).Key); 

            IsActive = false;
        }
    }

    public override void ChangePosition(bool isUp)
    {
        if(isUp)
        {
            if(_currentMenuPoint == 0)
            {
                return;
            }

            _currentMenuPoint--;
        }
        else
        {
            if(_currentMenuPoint == _availabeItems.Length)
            {
                return;
            }

            _currentMenuPoint++;
        }

        for (int i = 0; i < _pointers.Length; i++)
        {
            if(i == _currentMenuPoint)
            {
                _pointers[i].gameObject.SetActive(true);
                continue;
            }

            _pointers[i].gameObject.SetActive(false);
        }
        
        if(_currentMenuPoint == _availabeItems.Length)
        {
            _textBox.text = "Select no item and go back.";

            return;
        }
        
        ShowItemDescriptionAndSetPrompt(_availabeItems[_currentMenuPoint]);
    }

    public override void ShowItemDescriptionAndSetPrompt(Item item)
    {
        _textBox.text = item.Description;

        // switch(item.ItemType)
        // {
        //     case Item.ItemTypes.isUsable:
        //         _useOrEquipPrompt.text = "Use";

        //         break;

        //     case Item.ItemTypes.isEquipment:
        //         _useOrEquipPrompt.text = "Equip";

        //         break;
            
        //     case Item.ItemTypes.isCurrency:
        //         _useOrEquipPrompt.text = "Look At";

        //         break;
        // }
    }

    private void TryPurchase(Item item)
    {
        Item coin = new Coin();
        int currentCoins;

        // https://stackoverflow.com/questions/2829873/how-can-i-detect-if-this-dictionary-key-exists-in-c
        if(InventoryManager.Instance.Inventory.TryGetValue(coin, out currentCoins))
        {
            if( currentCoins >= item.StorePrice)
            {
                InventoryManager.Instance.Inventory[coin] = currentCoins - item.StorePrice;

                InventoryManager.Instance.ManageInventory(item, 1, true);

                return;
            }
            else
            {
                // nicht genug Münzen vorhanden ausgeben

                return;
            }
        }
        else
        {
            // keine Münzen vorhanden ausgeben

            return;
        }
    }
}
