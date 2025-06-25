using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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
    public List<Item> InventoryItems;
    public List<int> InventoryAmounts;
    public List<bool> EquipmentBools;
    public int NumberOfRings;
    public int NumberofAmulets;
    public int NumberofSwords;
    public int ConditionAmount;
    public bool IsDay;
    public bool IsSleepDeprived;
    public bool IsVampire;
    public bool IsBoostedVampire;
    public bool IsWerewolf;
    public bool IsZombie;

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
        public List<Item> InventoryItems;
        public List<int> InventoryAmounts;
        public List<bool> EquipmentBools;
        public int NumberOfRings;
        public int NumberofAmulets;
        public int NumberofSwords;
        public int ConditionAmount;
        public bool IsDay;
        public bool IsSleepDeprived;
        public bool IsVampire;
        public bool IsBoostedVampire;
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
        data.ConditionAmount = ConditionAmount;
        data.IsSleepDeprived = ConditionManager.Instance.IsSleepDeprived;
        data.IsVampire = ConditionManager.Instance.IsVampire;
        data.IsBoostedVampire = ConditionManager.Instance.IsBoostedVampire;
        data.IsWerewolf = ConditionManager.Instance.IsWerewolf;
        data.IsZombie = ConditionManager.Instance.IsZombie;

        InventoryItems.Clear();
        InventoryAmounts.Clear();
        EquipmentBools.Clear();

        foreach (Item item in InventoryManager.Instance.InventoryItems)
        {
            InventoryItems.Add(item);
            InventoryAmounts.Add(InventoryUtil.ReturnItemAmount(item));
        }

        if (InventoryManager.Instance.EquippedItems.Count > 0)
        {
            for (int i = 0; i < InventoryManager.Instance.InventoryItems.Count; i++)
            {
                EquipmentBools.Add(InventoryManager.Instance.EquippedItems.ElementAt(i).Value);
            }
        }

        data.EquipmentBools = EquipmentBools;
        data.NumberOfRings = NumberOfRings;
        data.NumberofAmulets = NumberofAmulets;
        data.NumberofSwords = NumberofSwords;
        data.InventoryItems = InventoryItems;
        data.InventoryAmounts = InventoryAmounts;
        data.IsDay = IsDay;

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);

        Debug.Log(json);
    }

    public void LoadAll()
    {
        string path = Application.persistentDataPath + "/savefile.json";

        if (File.Exists(path))
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
            EquipmentBools = data.EquipmentBools;
            NumberOfRings = data.NumberOfRings;
            NumberofAmulets = data.NumberofAmulets;
            NumberofSwords = data.NumberofSwords;
            InventoryItems = data.InventoryItems;
            InventoryAmounts = data.InventoryAmounts;
            IsDay = data.IsDay;
            ConditionAmount = data.ConditionAmount;
            IsSleepDeprived = data.IsSleepDeprived;
            IsVampire = data.IsVampire;
            IsBoostedVampire = data.IsBoostedVampire;
            IsWerewolf = data.IsWerewolf;
            IsZombie = data.IsZombie;

            if (InventoryManager.Instance != null)
            {
                for (int i = 0; i < data.InventoryItems.Count; i++)
                {
                    InventoryManager.Instance.InventoryItems.Add(data.InventoryItems[i]);
                    InventoryManager.Instance.InventoryAmounts.Add(data.InventoryAmounts[i]);
                }

                for (int i = 0; i < data.EquipmentBools.Count; i++)
                {
                    InventoryManager.Instance.EquippedItems.Add(i, EquipmentBools[i]);
                }
            }

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.InitializePlayerStats();
            }

            if (ConditionManager.Instance != null)
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
        EquipmentBools.Clear();
        NumberOfRings = 0;
        NumberofAmulets = 0;
        NumberofSwords = 0;
        InventoryItems.Clear();
        InventoryAmounts.Clear();
        IsSleepDeprived = false;
        IsVampire = false;
        IsBoostedVampire = false;
        IsWerewolf = false;
        IsZombie = false;

        PlayerManager.Instance.ResetStats();
        ConditionManager.Instance.ResetStats();
        InventoryManager.Instance.ResetStats();

        SaveAll();
    }

    private void CreateEmptySave()
    {
        SaveData data = new SaveData();

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);

        Debug.Log(json);
    }
}
