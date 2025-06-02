using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryDisplayer : SelectionMenu
{
    [SerializeField] private TextMeshProUGUI[] _amountTexts;
    [SerializeField] private TextMeshProUGUI[] _equipIndicators;
    [SerializeField] protected TextMeshProUGUI _textBox;
    [SerializeField] private TextMeshProUGUI _useOrEquipPrompt;
    [SerializeField] private RestingManager _restingManager;
    public delegate void ItemSelection(Item item);
    public static ItemSelection itemSelection;
    protected string _name;
    private List<Equipment> _tempEquipment;
    private int _amount;
    private int _ringCount = 0;
    private int _amuletCount = 0;
    private int _swordCount = 0;

    public override void Start()
    {
        IsActive = true;

        InitializeInventory();

        if (InventoryManager.Instance.InventoryItems.Count > 0)
        {
            _textBox.text = InventoryManager.Instance.InventoryItems.ElementAt(_currentMenuPoint).Description;
        }

        SetInitialPointer();

        UpdateEquipIndicators();
    }

    void Update()
    {
        ListenForInputs();
    }

    private void InitializeInventory()
    {
        for (int i = 0; i < InventoryManager.Instance.InventoryItems.Count; i++)
        {
            _name = InventoryManager.Instance.InventoryItems[i].Name;
            _amount = InventoryManager.Instance.InventoryAmounts[i];

            _name = DialogueUtil.AddEnding(InventoryManager.Instance.InventoryItems[i].Name, _amount);

            _menuPoints[i].text = _name;
            _amountTexts[i].text = $"x {_amount}";
        }

        _menuPoints[InventoryManager.Instance.InventoryItems.Count].text = "Return";
        _amountTexts[InventoryManager.Instance.InventoryItems.Count].text = "";

        for (int i = InventoryManager.Instance.InventoryItems.Count + 1; i < _menuPoints.Length; i++)
        {
            _menuPoints[i].text = "";
            _amountTexts[i].text = "";
            _pointers[i].enabled = false;
        }
    }

    public virtual void UpdateDisplayedInventory(Item item)
    {
        if (InventoryManager.Instance.InventoryItems.Count <= 0)
        {
            _menuPoints[0].text = "Return";
            _amountTexts[0].text = "";
        }

        if (InventoryManager.Instance.InventoryItems.Contains(item))
        {
            _amount = InventoryManager.Instance.InventoryAmounts.ElementAt(_currentMenuPoint);
            _amountTexts[_currentMenuPoint].text = $"x {_amount}";
        }
        else
        {
            for (int i = _currentMenuPoint; i < _menuPoints.Length; i++)
            {
                if (_menuPoints[i].text == "")
                {
                    break;
                }

                _menuPoints[i].text = _menuPoints[i + 1].text;
                _amountTexts[i].text = _amountTexts[i + 1].text;
            }

            if (InventoryManager.Instance.InventoryItems.Count > 0)
            {
                itemSelection?.Invoke(InventoryManager.Instance.InventoryItems.ElementAt(_currentMenuPoint));
            }
        }
    }

    public override void InitializeMenu()
    {
        base.InitializeMenu();

        if (InventoryManager.Instance.InventoryItems.Count <= 0)
        {
            _textBox.enabled = true;
            _textBox.text = "Your inventory is empty.";
            return;
        }

        _textBox.text = InventoryManager.Instance.InventoryItems.ElementAt(_currentMenuPoint).Description;

        if (InventoryManager.Instance.InventoryItems.ElementAt(_currentMenuPoint) != null)
        {
            ShowItemDescriptionAndSetPrompt(InventoryManager.Instance.InventoryItems[_currentMenuPoint]);
        }
    }

    public override void ListenForInputs()
    {
        while (!IsActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_currentMenuPoint == _menuPoints.Length - 1)
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
        else if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            if (_currentMenuPoint == InventoryManager.Instance.InventoryItems.Count || InventoryManager.Instance.InventoryItems.Count == 0)
            {
                _restingManager.ToggleCanvas(_restingManager.SelectionMenuCanvas, true);
                _restingManager.ToggleCanvas(_restingManager.InventoryCanvas, false);
                _restingManager.TogglePlayerStatsPosition(true);

                _textBox.text = "";

                SetInitialPointer();

                return;
            }

            _restingManager.ToggleCanvas(_restingManager.ItemToDoCanvas, true);
            _restingManager.ToggleCanvas(_restingManager.SelectionMenuCanvas, false);

            itemSelection?.Invoke(InventoryManager.Instance.InventoryItems[_currentMenuPoint]);

            IsActive = false;
        }
    }

    public override void ChangePosition(bool isUp)
    {
        if (isUp)
        {
            if (_currentMenuPoint == 0)
            {
                return;
            }

            _currentMenuPoint--;
        }
        else
        {
            if (_currentMenuPoint == InventoryManager.Instance.InventoryItems.Count)
            {
                return;
            }

            _currentMenuPoint++;
        }

        for (int i = 0; i < _pointers.Length; i++)
        {
            if (i == _currentMenuPoint)
            {
                _pointers[i].gameObject.SetActive(true);
                continue;
            }

            _pointers[i].gameObject.SetActive(false);
        }

        if (_currentMenuPoint == InventoryManager.Instance.InventoryItems.Count)
        {
            _textBox.text = "Select no item and go back.";

            return;
        }

        ShowItemDescriptionAndSetPrompt(InventoryManager.Instance.InventoryItems[_currentMenuPoint]);
    }

    public virtual void ShowItemDescriptionAndSetPrompt(Item item)
    {
        _textBox.text = item.Description;

        switch (item.ItemType)
        {
            case Item.ItemTypes.isUsable:
                _useOrEquipPrompt.text = "Use";

                break;

            case Item.ItemTypes.isEquipment:
                if (!InventoryManager.Instance.CurrentEquipment.Contains(item))
                {
                    _useOrEquipPrompt.text = "Equip";
                }
                else
                {
                    if (_equipIndicators[_currentMenuPoint].text == "")
                    {
                        _useOrEquipPrompt.text = "Equip";

                        break;
                    }

                    _useOrEquipPrompt.text = "Unequip";
                }

                break;

            case Item.ItemTypes.isCurrency:
                _useOrEquipPrompt.text = "Look At";

                break;
        }
    }

    public void UpdateEquipIndicators()
    {
        ResetTempEquipment();

        for (int i = 0; i < _equipIndicators.Length; i++)
        {
            if (i >= InventoryManager.Instance.InventoryItems.Count)
            {
                _equipIndicators[i].text = "";

                continue;
            }

            if (InventoryManager.Instance.InventoryItems[i] is not Equipment)
            {
                _equipIndicators[i].text = "";

                continue;
            }

            var equipment = (Equipment)InventoryManager.Instance.InventoryItems[i];

            if (InventoryManager.Instance.CurrentEquipment.Contains(equipment))
            {
                if (!CheckEquipment(equipment))
                {
                    _equipIndicators[i].text = "";

                    continue;
                }

                _equipIndicators[i].text = "E";
            }
            else
            {
                _equipIndicators[i].text = "";
            }
        }
    }

    private bool CheckEquipment(Equipment equipment)
    {
        if (_tempEquipment.Contains(equipment))
        {
            _tempEquipment.Remove(equipment);

            return true;
        }

        return false;
    }

    private void ResetTempEquipment()
    {
        //https://stackoverflow.com/questions/1952185/how-do-i-copy-items-from-list-to-list-without-foreach
        _tempEquipment = new List<Equipment>(InventoryManager.Instance.CurrentEquipment);
    }
}
