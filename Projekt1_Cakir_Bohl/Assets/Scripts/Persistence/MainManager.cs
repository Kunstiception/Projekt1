using UnityEngine;
using System.IO;
using System.Collections.Generic;

//https://learn.unity.com/tutorial/implement-data-persistence-between-scenes#634f8281edbc2a65c86270c7
//https://learn.unity.com/pathway/junior-programmer/unit/manage-scene-flow-and-data/tutorial/676202deedbc2a019fcfe5cc
public class MainManager : MonoBehaviour
{
    public static MainManager Instance;

    public int PlayerHealthPoints;
    public int PlayerEgoPoints;
    public int HealthPointsModifier;
    public int EgoPointsModifier;
    public int AttackStrengthModifier;
    public int InitiativeModifier;
    public int EvasionModifier;
    public int InsultResistenceModifier;
    public int AccuracyModifier;
    public int InsultDamageModifier;
    public int CurrentDay;
    public string LastWayPoint;
    // Leider kein Speichern von Dictionaries mit JSON-Utitlity möglich, daher zwei Listen
    public List<string> WayPoints;
    public List<int> WayPointTypes;
    public List<string> InventoryNames;
    public List<int> InventoryAmounts;
    public List<Equipment> CurrentEquipment;
    public bool IsDay;
    public bool IsSleepDeprived;
    public bool IsVampire;
    public bool IsWerewolf;
    public bool IsZombie;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [System.Serializable]
    class SaveData
    {
        public int PlayerHealthPoints;
        public int PlayerEgoPoints;
        public int HealthPointsModifier;
        public int EgoPointsModifier;
        public int AttackStrengthModifier;
        public int InitiativeModifier;
        public int EvasionModifier;
        public int InsultResistenceModifier;
        public int AccuracyModifier;
        public int InsultDamageModifier;
        public int CurrentDay;
        public string LastWayPoint;
        public List<string> WayPoints;
        public List<int> WayPointTypes;
        public List<string> InventoryNames;
        public List<int> InventoryAmounts;
        public List<Equipment> CurrentEquipment;
        public bool IsDay;
        public bool IsSleepDeprived;
        public bool IsVampire;
        public bool IsWerewolf;
        public bool IsZombie;
    }

    public void SaveAll()
    {
        SaveData data = new SaveData();

        data.PlayerHealthPoints = PlayerManager.Instance.HealthPoints - PlayerManager.Instance.HealthPointsModifier;
        data.PlayerEgoPoints = PlayerManager.Instance.EgoPoints - PlayerManager.Instance.EgoPointsModifier;
        data.HealthPointsModifier = PlayerManager.Instance.HealthPointsModifier;
        data.EgoPointsModifier = PlayerManager.Instance.EgoPointsModifier;
        data.AttackStrengthModifier = PlayerManager.Instance.AttackStrengthModifier;
        data.InitiativeModifier = PlayerManager.Instance.InitiativeModifier;
        data.EvasionModifier = PlayerManager.Instance.EvasionModifier;
        data.InsultResistenceModifier = PlayerManager.Instance.InsultResistanceModifier;
        data.AccuracyModifier = PlayerManager.Instance.AccuracyModifier;
        data.InsultDamageModifier = PlayerManager.Instance.InsultDamageModifier;
        data.CurrentDay = CurrentDay;
        data.LastWayPoint = LastWayPoint;
        data.WayPoints = WayPoints;
        data.WayPointTypes = WayPointTypes;
        data.IsSleepDeprived = ConditionManager.Instance.IsSleepDeprived;
        data.IsVampire = ConditionManager.Instance.IsVampire;
        data.IsWerewolf = ConditionManager.Instance.IsWerewolf;
        data.IsZombie = ConditionManager.Instance.IsZombie;

        InventoryNames.Clear();
        InventoryAmounts.Clear();

        foreach(Item item in InventoryManager.Instance.Inventory.Keys)
        {
            InventoryNames.Add(item.name);
            InventoryAmounts.Add(InventoryManager.Instance.Inventory[item]);
        }

        foreach (Equipment equipment in InventoryManager.Instance.CurrentEquipment)
        {
            CurrentEquipment.Add(equipment);
        }

        data.CurrentEquipment = CurrentEquipment;
        data.InventoryNames = InventoryNames;
        data.InventoryAmounts = InventoryAmounts;
        data.IsDay = IsDay;

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);

        Debug.Log(json);
    }

    public void LoadAll()
    {
        string path = Application.persistentDataPath + "/savefile.json";

        if(File.Exists(path))
        {
            string json = File.ReadAllText(path);

            SaveData data = JsonUtility.FromJson<SaveData>(json);

            PlayerHealthPoints = data.PlayerHealthPoints;
            PlayerEgoPoints = data.PlayerEgoPoints;
            HealthPointsModifier = data.HealthPointsModifier;
            EgoPointsModifier = data.EgoPointsModifier;
            AttackStrengthModifier = data.AccuracyModifier;
            InitiativeModifier = data.InitiativeModifier;
            EvasionModifier = data.EvasionModifier;
            InsultResistenceModifier = data.InsultResistenceModifier;
            AccuracyModifier = data.AccuracyModifier;
            InsultDamageModifier = data.InsultDamageModifier;
            CurrentDay = data.CurrentDay;
            LastWayPoint = data.LastWayPoint;
            WayPoints = data.WayPoints;
            WayPointTypes = data.WayPointTypes;
            CurrentEquipment = data.CurrentEquipment;
            InventoryNames = data.InventoryNames;
            InventoryAmounts = data.InventoryAmounts;
            IsDay = data.IsDay;
            IsSleepDeprived = data.IsSleepDeprived;
            IsVampire = data.IsVampire;
            IsWerewolf = data.IsWerewolf;
            IsZombie = data.IsZombie;

            if (InventoryManager.Instance != null)
            {
                for (int i = 0; i < data.InventoryNames.Count; i++)
                {
                    InventoryManager.Instance.Inventory.Add(InventoryManager.Instance.AllItems[data.InventoryNames[i]],
                        data.InventoryAmounts[i]);
                }

                for (int i = 0; i < data.CurrentEquipment.Count; i++)
                {
                    InventoryManager.Instance.CurrentEquipment.Add(CurrentEquipment[i]);
                }
            }

            if(PlayerManager.Instance != null)
            {
                PlayerManager.Instance.InitializePlayerStats();
            }

            if(ConditionManager.Instance != null)
            {
                ConditionManager.Instance.InitializeConditions();
            }

            Debug.Log(json);
        }  
    }

    public void RevertAll()
    {
        PlayerHealthPoints = GameConfig.PlayerStartingHealth;
        PlayerEgoPoints = GameConfig.PlayerStartingEgo;
        CurrentDay = 0;
        LastWayPoint = "";
        IsDay = true;
        HealthPointsModifier = 0;
        EgoPointsModifier = 0;
        AttackStrengthModifier = 0;
        InitiativeModifier = 0;
        EvasionModifier = 0;
        InsultResistenceModifier = 0;
        AccuracyModifier = 0;
        InsultDamageModifier = 0;
        WayPoints.Clear();
        WayPointTypes.Clear();
        InventoryNames.Clear();
        InventoryAmounts.Clear();
        IsSleepDeprived = false;
        IsVampire = false;
        IsWerewolf = false;
        IsZombie = false;
    }
}
