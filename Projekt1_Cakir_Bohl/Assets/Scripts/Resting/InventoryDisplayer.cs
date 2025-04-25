using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryDisplayer : SelectionMenu
{
    [SerializeField] private TextMeshProUGUI[] _amountTexts;
    [SerializeField] private TextMeshProUGUI _textBox;

    void Start()
    {
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
        int amount = 0;
        string name;

        for(int i = 0; i < InventoryManager.Instance.Inventory.Count; i++)
        {
            name = InventoryManager.Instance.Inventory.ElementAt(i).Key.Name;
            amount = InventoryManager.Instance.Inventory[InventoryManager.Instance.Inventory.ElementAt(i).Key];

            name = DialogueUtil.AddEnding(InventoryManager.Instance.Inventory.ElementAt(i).Key.Name, amount);

            _menuPoints[i].text = name;
            _amountTexts[i].text = $"x {amount}";
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

    public override void ListenForInputs()
    {
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
            //_iSelectable.HandleSelectedItem(_currentMenuPoint, isFirstLayer: _isFirstLayer);
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
