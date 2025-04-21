using System.Collections.Generic;

public class PlayerManager: Combatant
{
    public static PlayerManager Instance;

    public bool HasDisadvantage = false;
    public Dictionary<Item, int> Inventory = new Dictionary<Item, int>();

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

    public void InitializeDefaultStats()
    {
        HealthPoints = MainManager.Instance.PlayerHealthPoints;
        EgoPoints = MainManager.Instance.PlayerEgoPoints;
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
