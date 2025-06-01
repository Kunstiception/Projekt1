using System.Linq;
using TMPro;
using UnityEngine;

public class Merchant : InventoryDisplayer
{
    [SerializeField] private Item[] _availabeItems;
    [SerializeField] private TextMeshProUGUI[] _priceTexts;
    [SerializeField] private InteractionManager _interactionManager;
    public delegate void OnTryPurchase();
    public static OnTryPurchase onTryPurchase;
    private int _price;

    public override void Start()
    {
        return;
    }

    void Update()
    {
        ListenForInputs();
    }

    public override void InitializeMenu()
    {
        IsActive = true;

        for (int i = 0; i < _availabeItems.Length; i++)
        {
            _name = _availabeItems.ElementAt(i).Name;
            _price = _availabeItems.ElementAt(i).StorePrice;

            _menuPoints[i].text = _name;
            _priceTexts[i].text = $"{_price} G";
        }

        _menuPoints[_availabeItems.Length].text = "Return";
        _priceTexts[_availabeItems.Length].text = "";

        for (int i = _availabeItems.Length + 1; i < _menuPoints.Length; i++)
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
        if(_availabeItems.Length <= 0)
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
            if(_currentMenuPoint == _availabeItems.Length || _availabeItems.Length == 0)
            {
                _interactionManager.ToggleCanvas(_interactionManager.InitialMenuCanvas, true);
                _interactionManager.ToggleCanvas(_interactionManager.MerchantInventoryCanvas, false);
                _interactionManager.ToggleCanvas(_interactionManager.ItemToDoCanvas, false);

                _textBox.text = "";

                SetInitialPointer();

                return;
            } 

            itemSelection?.Invoke(_availabeItems.ElementAt(_currentMenuPoint));

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
    }
}
