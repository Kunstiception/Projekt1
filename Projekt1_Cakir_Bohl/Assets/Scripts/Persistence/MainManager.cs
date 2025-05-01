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
    public string LastWayPoint;
    public List<string> VisitedWayPoints;
    public List<string> InventoryNames;
    public List<int> InventoryAmounts;

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
        public string LastWayPoint;
        public List<string> VisitedWayPoints;
        public List<string> InventoryNames;
        public List<int> InventoryAmounts;
    }

    public void SaveAll()
    {
        SaveData data = new SaveData();

        data.PlayerHealthPoints = PlayerManager.Instance.HealthPoints;
        data.PlayerEgoPoints = PlayerManager.Instance.EgoPoints;
        data.LastWayPoint = LastWayPoint;
        data.VisitedWayPoints = VisitedWayPoints;

        InventoryNames.Clear();
        InventoryAmounts.Clear();

        foreach(Item item in InventoryManager.Instance.Inventory.Keys)
        {
            InventoryNames.Add(item.name);
            InventoryAmounts.Add(InventoryManager.Instance.Inventory[item]);
        }
        
        data.InventoryNames = InventoryNames;
        data.InventoryAmounts = InventoryAmounts;

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
            LastWayPoint = data.LastWayPoint;
            VisitedWayPoints = data.VisitedWayPoints;
            InventoryNames = data.InventoryNames;
            InventoryAmounts = data.InventoryAmounts;

            if(InventoryManager.Instance != null)
            {
                for(int i = 0; i < data.InventoryNames.Count; i++)
                {
                    InventoryManager.Instance.Inventory.Add(InventoryManager.Instance.AllItems[data.InventoryNames[i]], 
                        data.InventoryAmounts[i]);
                }
            }

            if(PlayerManager.Instance != null)
            {
                PlayerManager.Instance.InitializePlayerStats();
            }

            Debug.Log(json);
        }
        
    }
}
