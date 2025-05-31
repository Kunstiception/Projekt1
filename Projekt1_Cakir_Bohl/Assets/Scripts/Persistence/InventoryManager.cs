using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Dictionary<Item, int> Inventory = new Dictionary<Item, int>();
    public Dictionary<string, Item> AllItems = new Dictionary<string, Item>();
    public List<Equipment> CurrentEquipment = new List<Equipment>();
    [SerializeField] private Item[] _items;
    public int TotalNumberOfItems;
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
        if (TotalNumberOfItems >= GameConfig.InventorySlots)
        {
            return;
        }

        if (Inventory.ContainsKey(item))
        {
            int currentCount = Inventory[item];

            if (isAdding)
            {
                if (item is Equipment)
                {
                    TotalNumberOfItems++;
                }

                Inventory[item] = currentCount + amount;

                return;
            }

            Inventory[item] = currentCount - amount;

            if (Inventory[item] <= 0)
            {
                Inventory.Remove(item);
            }

            TotalNumberOfItems--;

            return;
        }

        Inventory.Add(item, amount);

        TotalNumberOfItems++;
    }

    public bool ManageEquipment(Item selectedEquipment, bool isEquip)
    {
        Equipment equipment = (Equipment)selectedEquipment;

        switch (equipment.equipmentType)
        {
            case Equipment.EquipmentType.isRing:
                if (!isEquip && _numberOfRings > 0)
                {
                    _numberOfRings--;

                    CurrentEquipment.Remove(equipment);

                    return true;
                }

                if (_numberOfRings >= 2)
                {
                    return false;
                }

                _numberOfRings++;

                break;

            case Equipment.EquipmentType.isAmulet:
                if (!isEquip && _numberOfArmor > 0)
                {
                    _numberOfArmor--;

                    CurrentEquipment.Remove(equipment);

                    return true;
                }

                if (_numberOfArmor >= 1)
                {
                    return false;
                }

                _numberOfArmor++;

                break;

            case Equipment.EquipmentType.isWeapon:
                if (!isEquip && _numberofHelmets > 0)
                {
                    _numberofHelmets--;

                    CurrentEquipment.Remove(equipment);

                    return true;
                }

                if (_numberofHelmets >= 1)
                {
                    return false;
                }

                _numberofHelmets++;

                break;

            default:
                return false;
        }

        CurrentEquipment.Add(equipment);

        return true;
    }
}
