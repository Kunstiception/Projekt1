using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // Zwei parallele Listen und kein Dictionary (mehr), da Equipment doppelt vorkommen soll
    public List<Item> InventoryItems = new List<Item>();
    public List<int> InventoryAmounts = new List<int>();
    public Dictionary<string, Item> AllItems = new Dictionary<string, Item>();
    public List<Equipment> CurrentEquipment = new List<Equipment>();
    [SerializeField] private Item[] _items;
    public int NumberOfRings;
    public int NumberOfAmulets;
    public int NumberofSwords;

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
        if (InventoryItems.Count >= GameConfig.InventorySlots)
        {
            return;
        }

        if (InventoryItems.Contains(item))
        {
            int currentCount = InventoryUtil.ReturnItemAmount(item);

            if (isAdding)
            {
                if (item is Equipment)
                {
                    InventoryItems.Add(item);
                    InventoryAmounts.Add(amount);

                    return;
                }

                InventoryAmounts[InventoryItems.IndexOf(item)] = currentCount + amount;

                return;
            }

            InventoryAmounts[InventoryItems.IndexOf(item)] = currentCount - amount;

            if (InventoryUtil.ReturnItemAmount(item) <= 0)
            {
                InventoryItems.Remove(item);
                InventoryAmounts.Remove(InventoryAmounts[InventoryItems.IndexOf(item)]);
            }

            // TotalNumberOfItems--;

            return;
        }

        InventoryItems.Add(item);
        InventoryAmounts.Add(amount);

        //TotalNumberOfItems++;
    }

    public bool ManageEquipment(Item selectedEquipment, bool isEquip)
    {
        Equipment equipment = (Equipment)selectedEquipment;

        switch (equipment.equipmentType)
        {
            case Equipment.EquipmentType.isRing:
                if (!isEquip && NumberOfRings > 0)
                {
                    NumberOfRings--;

                    CurrentEquipment.Remove(equipment);

                    return true;
                }

                if (NumberOfRings >= 2)
                {
                    return false;
                }

                NumberOfRings++;

                break;

            case Equipment.EquipmentType.isAmulet:
                if (!isEquip && NumberOfAmulets > 0)
                {
                    NumberOfAmulets--;

                    CurrentEquipment.Remove(equipment);

                    return true;
                }

                if (NumberOfAmulets >= 1)
                {
                    return false;
                }

                NumberOfAmulets++;

                break;

            case Equipment.EquipmentType.isSword:
                if (!isEquip && NumberofSwords > 0)
                {
                    NumberofSwords--;

                    CurrentEquipment.Remove(equipment);

                    return true;
                }

                if (NumberofSwords >= 1)
                {
                    return false;
                }

                NumberofSwords++;

                break;

            default:
                return false;
        }

        CurrentEquipment.Add(equipment);

        return true;
    }
}
