using UnityEngine;
using System.IO;
using System.Collections.Generic;

//https://learn.unity.com/tutorial/implement-data-persistence-between-scenes#634f8281edbc2a65c86270c7
//https://learn.unity.com/pathway/junior-programmer/unit/manage-scene-flow-and-data/tutorial/676202deedbc2a019fcfe5cc
public class MainManager : MonoBehaviour
{
    public static MainManager Instance;

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

        LoadAll();
    }

    [System.Serializable]
    class SaveData
    {
        public string LastWayPoint;
        public List<string> VisitedWayPoints;
    }

    public void SaveAll()
    {
        SaveData data = new SaveData();
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

            LastWayPoint = data.LastWayPoint;
            VisitedWayPoints = data.VisitedWayPoints;
            Debug.Log(json);
        }
        
    }
}
