using System.Collections.Generic;

public class PlayerManager: Combatant
{
    public static PlayerManager Instance;

    public bool HasDisadvantage = false;

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
}
