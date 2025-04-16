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

    public void InitializeStatsOnLoad()
    {
        HealthPoints = MainManager.Instance.PlayerHealthPoints;
        EgoPoints = MainManager.Instance.PlayerEgoPoints;
    }
}
