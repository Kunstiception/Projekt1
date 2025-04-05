using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager: Combatant
{
    public static PlayerManager Instance;

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
}
