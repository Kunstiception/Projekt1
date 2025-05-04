using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Dictionary<Item, int> Inventory = new Dictionary<Item, int>();
    public Dictionary<string, Item> AllItems = new Dictionary<string, Item>();
    [SerializeField] Item[] _items;

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
        foreach(Item item in _items)
        {
            AllItems.Add(item.name, item);
        }
    }


    public void ManageInventory(Item item, int amount, bool isAdding)
    {
         if(Inventory.ContainsKey(item))
        {
            int currentCount = Inventory[item];

            if(isAdding)
            {
                Inventory[item] = currentCount + amount;
                return;
            }

            Inventory[item] = currentCount - amount;

            if(Inventory[item] <= 0)
            {
                Inventory.Remove(item);
            }
            
            return;
        }

        Inventory.Add(item, amount);
    }
}
