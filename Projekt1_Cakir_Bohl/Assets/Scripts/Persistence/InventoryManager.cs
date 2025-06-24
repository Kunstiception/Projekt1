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

    public void ResetStats()
    {
        InventoryItems.Clear();
        InventoryAmounts.Clear();
        EquippedItems.Clear();
    }

    public void ManageInventory(Item item, int amount, bool isAdding, int index = 0)
    {
        if (InventoryItems.Contains(item))
        {
            int currentCount = InventoryUtil.ReturnItemAmount(item);

            if (isAdding)
            {
                if (item is Equipment)
                {
                    if (InventoryItems.Count >= GameConfig.MaxInventorySlots)
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
                if (item is Equipment)
                {
                    ManageEquipment(item, false, index);

                    UpdateEquipBools(index);
                }

                InventoryAmounts.Remove(InventoryAmounts[InventoryItems.IndexOf(item)]);
                InventoryItems.Remove(item);
            }

            return;
        }

        if (InventoryItems.Count >= GameConfig.MaxInventorySlots)
        {
            return;
        }

        InventoryItems.Add(item);
        InventoryAmounts.Add(amount);

        EquippedItems.Add(InventoryItems.Count - 1, false);
    }

    public void UpdateEquipBools(int index)
    {
        for (int i = index + 1; i < EquippedItems.Count; i++)
        {
            var kvp = new KeyValuePair<int, bool>(EquippedItems.ElementAt(i).Key - 1, EquippedItems.ElementAt(i).Value);

            EquippedItems.Remove(i - 1);

            EquippedItems.Add(kvp.Key, kvp.Value);
        }
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
