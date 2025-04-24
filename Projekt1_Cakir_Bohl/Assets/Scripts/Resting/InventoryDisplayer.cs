using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryDisplayer : SelectionMenu
{
    [SerializeField] private TextMeshProUGUI[] _amountTexts;

    void Start()
    {
        InitializeInventory();

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

        for(int i = 0; i < PlayerManager.Instance.Inventory.Count; i++)
        {
            name = PlayerManager.Instance.Inventory.ElementAt(i).Key.Name;
            amount = PlayerManager.Instance.Inventory[PlayerManager.Instance.Inventory.ElementAt(i).Key];

            name = DialogueUtil.AddEnding(PlayerManager.Instance.Inventory.ElementAt(i).Key.Name, amount);

            _menuPoints[i].text = name;
            _amountTexts[i].text = $"x {amount}";
        }

        _menuPoints[PlayerManager.Instance.Inventory.Count].text = "Return";
        _amountTexts[PlayerManager.Instance.Inventory.Count].text = "";

        for(int i = PlayerManager.Instance.Inventory.Count + 1; i < _menuPoints.Length; i++)
        {
            _menuPoints[i].text = "";
            _amountTexts[i].text = "";
            _pointers[i].enabled = false;
        }
    }
}
