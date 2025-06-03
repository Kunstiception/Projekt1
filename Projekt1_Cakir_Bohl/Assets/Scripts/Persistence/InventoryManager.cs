using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // Zwei parallele Listen und kein Dictionary (mehr), da Equipment doppelt vorkommen soll
    public List<Item> InventoryItems = new List<Item>();
    public List<int> InventoryAmounts = new List<int>();
    public Dictionary<string, Item> AllItems = new Dictionary<string, Item>();
    public Dictionary<int, bool> EquippedItems = new Dictionary<int, bool>();
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
            AllItems.Add(item.Name, item);
        }
    }

    public void ManageInventory(Item item, int amount, bool isAdding)
    {
        if (InventoryItems.Contains(item))
        {
            int currentCount = InventoryUtil.ReturnItemAmount(item);

            if (isAdding)
            {
                if (item is Equipment)
                {
                    if (InventoryItems.Count >= GameConfig.InventorySlots)
                    {
                        return;
                    }

                    InventoryItems.Add(item);
                    InventoryAmounts.Add(amount);

                    EquippedItems.Add(InventoryItems.Count - 1, false);

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

                EquippedItems.Remove(InventoryItems.IndexOf(item));
            }

            return;
        }

        if (InventoryItems.Count >= GameConfig.InventorySlots)
        {
            return;
        }

        InventoryItems.Add(item);
        InventoryAmounts.Add(amount);

        EquippedItems.Add(InventoryItems.Count - 1, false);
    }

    public bool ManageEquipment(Item selectedEquipment, bool isEquip, int inventoryIndex)
    {
        Equipment equipment = (Equipment)selectedEquipment;

        switch (equipment.equipmentType)
        {
            case Equipment.EquipmentType.IsRing:
                if (!isEquip && NumberOfRings > 0)
                {
                    NumberOfRings--;

                    EquippedItems[inventoryIndex] = false;

                    return true;
                }

                if (NumberOfRings >= 2)
                {
                    return false;
                }

                NumberOfRings++;

                break;

            case Equipment.EquipmentType.IsAmulet:
                if (!isEquip && NumberOfAmulets > 0)
                {
                    NumberOfAmulets--;

                    EquippedItems[inventoryIndex] = false;

                    return true;
                }

                if (NumberOfAmulets >= 1)
                {
                    return false;
                }

                NumberOfAmulets++;

                break;

            case Equipment.EquipmentType.IsSword:
                if (!isEquip && NumberofSwords > 0)
                {
                    NumberofSwords--;

                    EquippedItems[inventoryIndex] = false;

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

        EquippedItems[inventoryIndex] = true;

        return true;
    }
}
