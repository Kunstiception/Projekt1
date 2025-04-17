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
    }

    public void SaveAll()
    {
        SaveData data = new SaveData();

        data.PlayerHealthPoints = PlayerManager.Instance.HealthPoints;
        data.PlayerEgoPoints = PlayerManager.Instance.EgoPoints;
        data.LastWayPoint = LastWayPoint;
        data.VisitedWayPoints = VisitedWayPoints;

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

            if(PlayerManager.Instance != null)
            {
                PlayerManager.Instance.InitializeStatsOnLoad();
            }

            Debug.Log(json);
        }
        
    }
}
