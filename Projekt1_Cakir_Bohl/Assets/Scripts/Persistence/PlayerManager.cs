using System.Collections.Generic;

public class PlayerManager: Combatant
{
    public static PlayerManager Instance;

    public bool HasDisadvantage = false;
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();

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

    public void InitializePlayerStats()
    {
        HealthPoints = MainManager.Instance.PlayerHealthPoints;
        EgoPoints = MainManager.Instance.PlayerEgoPoints;
    }

    public void ManageInventory(Item item, int amount, bool isAdding)
    {
         if(Inventory.ContainsKey(item.name))
        {
            int currentCount = Inventory[item.name];

            if(isAdding)
            {
                Inventory[item.name] = currentCount + amount;
                return;
            }

            Inventory[item.name] = currentCount - amount;

            if(Inventory[item.name] <= 0)
            {
                Inventory.Remove(item.name);
            }
            
            return;
        }

        Inventory.Add(item.name, amount);
    }
}
