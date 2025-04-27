using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryDisplayer : SelectionMenu
{
    public bool IsActive;
    [SerializeField] private TextMeshProUGUI[] _amountTexts;
    [SerializeField] private TextMeshProUGUI _textBox;
    [SerializeField] private RestingManager _restingManager;

    public delegate void ItemSelection(Item item);
    public static ItemSelection itemSelection;
    private string _name;
    private int _amount;

    void Start()
    {
        IsActive = true;
        
        InitializeInventory();

        _textBox.text = InventoryManager.Instance.Inventory.ElementAt(_currentMenuPoint).Key.Description;

        SetInitialPointer();
    }

    void Update()
    {
        ListenForInputs();
    }

    private void InitializeInventory()
    {
        for(int i = 0; i < InventoryManager.Instance.Inventory.Count; i++)
        {
            _name = InventoryManager.Instance.Inventory.ElementAt(i).Key.Name;
            _amount = InventoryManager.Instance.Inventory[InventoryManager.Instance.Inventory.ElementAt(i).Key];

            _name = DialogueUtil.AddEnding(InventoryManager.Instance.Inventory.ElementAt(i).Key.Name, _amount);

            _menuPoints[i].text = _name;
            _amountTexts[i].text = $"x {_amount}";
        }

        _menuPoints[InventoryManager.Instance.Inventory.Count].text = "Return";
        _amountTexts[InventoryManager.Instance.Inventory.Count].text = "";

        for(int i = InventoryManager.Instance.Inventory.Count + 1; i < _menuPoints.Length; i++)
        {
            _menuPoints[i].text = "";
            _amountTexts[i].text = "";
            _pointers[i].enabled = false;
        }
    }

    public void UpdateDisplayedInventory()
    {            
        _amount = InventoryManager.Instance.Inventory.ElementAt(_currentMenuPoint).Value;

        if(_amount <= 0)
        {
            for(int i = _currentMenuPoint; i < _menuPoints.Length; i++)
            {
                if(_menuPoints[i + 1].text == "")
                {
                    break;
                }

                _menuPoints[i].text = _menuPoints[i + 1].text;
                _amountTexts[i].text = _amountTexts[i + 1].text;
            }
        }

        _amountTexts[_currentMenuPoint].text = $"x {_amount}";
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
            if(_currentMenuPoint == InventoryManager.Instance.Inventory.Count)
            {
                _restingManager.ToggleCanvas(_restingManager.SelectionMenuCanvas, true);
                _restingManager.ToggleCanvas(_restingManager.InventoryCanvas, false);

                _textBox.text = "";

                SetInitialPointer();

                return;
            }

            _restingManager.ToggleCanvas(_restingManager.ItemToDoCanvas, true);
            _restingManager.ToggleCanvas(_restingManager.SelectionMenuCanvas, false);

            itemSelection?.Invoke(InventoryManager.Instance.Inventory.ElementAt(_currentMenuPoint).Key); 

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
            if(_currentMenuPoint == InventoryManager.Instance.Inventory.Count)
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
        
        if(_currentMenuPoint == InventoryManager.Instance.Inventory.Count)
        {
            _textBox.text = "Select no item and go back.";

            return;
        }
        
        ShowItemDescription(InventoryManager.Instance.Inventory.ElementAt(_currentMenuPoint).Key);
    }

    private void ShowItemDescription(Item item)
    {
        _textBox.text = item.Description;
    }
}
