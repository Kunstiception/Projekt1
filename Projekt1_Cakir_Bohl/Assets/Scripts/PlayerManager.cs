using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager: MonoBehaviour
{
    public static PlayerManager Instance;
    
    public int HealthPoints = 10;
    public int MinAttackStrength = 4;
    public int MaxAttackStrength = 7;
    public int ArmorStrength = 10;
    public int Initiative = 10;

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
