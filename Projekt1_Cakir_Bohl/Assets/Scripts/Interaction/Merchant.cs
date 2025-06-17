using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Merchant : InventoryDisplayer
{
    [SerializeField] private List<Item> _availabeItems = new List<Item>();
    [SerializeField] private Item[] _availableEquipment;
    [SerializeField] private TextMeshProUGUI[] _priceTexts;
    [SerializeField] private InteractionManager _interactionManager;
    public delegate void OnTryPurchase();
    public static OnTryPurchase onTryPurchase;
    private List<Item> _tempAdditionalEquipment = new List<Item>();
    private int _price;

    public override void Start()
    {
        return;
    }

    void Update()
    {
        ListenForInputs();
    }

    private void AddEquipment()
    {
        foreach (Item item in _availableEquipment)
        {
            _tempAdditionalEquipment.Add(item);
        }

        for (int i = 0; i < GameConfig.EquipmentToAdd; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, _tempAdditionalEquipment.Count);

            _availabeItems.Add(_tempAdditionalEquipment[randomIndex]);

            _tempAdditionalEquipment.RemoveAt(randomIndex);
        }
    }

    public override void InitializeMenu()
    {
        AddEquipment();

        IsActive = true;

        for (int i = 0; i < _availabeItems.Count; i++)
        {
            _name = _availabeItems.ElementAt(i).Name;
            _price = _availabeItems.ElementAt(i).StorePrice;

            _menuPoints[i].text = _name;
            _priceTexts[i].text = $"{_price} G";
        }

        _menuPoints[_availabeItems.Count].text = "Return";
        _priceTexts[_availabeItems.Count].text = "";

        for (int i = _availabeItems.Count + 1; i < _menuPoints.Length; i++)
        {
            _menuPoints[i].text = "";
            _priceTexts[i].text = "";
            _pointers[i].enabled = false;
        }

        _textBox.text = _availabeItems.ElementAt(_currentMenuPoint).Description;

        SetInitialPointer();
    }

    public override void UpdateDisplayedInventory(Item item)
    {            
        if(_availabeItems.Count <= 0)
        {
            _menuPoints[0].text = "Return";
            _priceTexts[0].text = "";
        }
        
        if(InventoryManager.Instance.InventoryItems.Contains(item))
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
            if(_currentMenuPoint == _availabeItems.Count || _availabeItems.Count == 0)
            {
                _interactionManager.ToggleCanvas(_interactionManager.InitialMenuCanvas, true);
                _interactionManager.ToggleCanvas(_interactionManager.MerchantInventoryCanvas, false);
                _interactionManager.ToggleCanvas(_interactionManager.ItemToDoCanvas, false);
                _interactionManager.TogglePlayerStatsPosition(true);

                _textBox.text = "";

                SetInitialPointer();

                return;
            } 

            itemSelection?.Invoke(_availabeItems.ElementAt(_currentMenuPoint), _currentMenuPoint);

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
            if(_currentMenuPoint == _availabeItems.Count)
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
        
        if(_currentMenuPoint == _availabeItems.Count)
        {
            _textBox.text = "Select no item and go back.";

            return;
        }
        
        ShowItemDescriptionAndSetPrompt(_availabeItems[_currentMenuPoint]);
    }

    public override void ShowItemDescriptionAndSetPrompt(Item item)
    {
        _textBox.text = item.Description;
    }
}
