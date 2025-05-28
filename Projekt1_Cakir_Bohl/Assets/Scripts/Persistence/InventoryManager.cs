using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Dictionary<Item, int> Inventory = new Dictionary<Item, int>();
    public Dictionary<string, Item> AllItems = new Dictionary<string, Item>();
    [SerializeField] private Item[] _items;
    private int _numberOfRings;
    private int _numberOfArmor;
    private int _numberofHelmets;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        foreach (Item item in _items)
        {
            AllItems.Add(item.name, item);
        }
    }

    public void ManageInventory(Item item, int amount, bool isAdding)
    {
        if (Inventory.ContainsKey(item))
        {
            int currentCount = Inventory[item];

            if (isAdding)
            {
                Inventory[item] = currentCount + amount;
                return;
            }

            Inventory[item] = currentCount - amount;

            if (Inventory[item] <= 0)
            {
                Inventory.Remove(item);
            }

            return;
        }

        Inventory.Add(item, amount);
    }

    public void ManageEquipment(Item selectedEquipment, bool isEquip)
    {
        Equipment equipment = (Equipment)selectedEquipment;

        switch (equipment.equipmentType)
        {
            case Equipment.EquipmentType.isRing:
                if (!isEquip)
                {
                    _numberOfRings--;

                    return;
                }

                if (_numberOfRings >= 2)
                {
                    return;
                }

                _numberOfRings++;

                break;

            case Equipment.EquipmentType.isBodyArmor:
                if (!isEquip)
                {
                    _numberOfArmor--;

                    return;
                }

                if (_numberOfArmor >= 1)
                {
                    return;
                }

                _numberOfArmor++;

                break;

            case Equipment.EquipmentType.isHelmet:
                if (!isEquip)
                {
                    _numberofHelmets--;

                    return;
                }

                if (_numberofHelmets >= 1)
                {
                    return;
                }

                _numberofHelmets++;

                break;
        }
    }
}
