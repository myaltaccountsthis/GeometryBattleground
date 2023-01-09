using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class DataManager : MonoBehaviour
{
    public int highScore;
    public int score;
    public int wave;
    public int level;
    public int experience;
    public float health;
    public Dictionary<string, int> ownedProjectiles;
    public Dictionary<string, int> ownedPassives;

    private string FILE_PATH;

    void Awake() {
        FILE_PATH = Application.persistentDataPath + "/gamedata.dat";
        LoadData();
    }

    void OnDestroy() {
        // SaveData();
    }

    public void LoadDefaultNonPersistent() {
        score = 0;
        wave = 0;
        level = 1;
        experience = 0;
        health = 100f;
        ownedProjectiles = new Dictionary<string, int>();
        ownedProjectiles["Ball"] = 1;
        ownedPassives = new Dictionary<string, int>();
    }

    void LoadData() {
        // default data
        highScore = 0;
        LoadDefaultNonPersistent();

        // NOTE: maybe try to change this class' instance variables at the end of each wave so the player stores the current values in memory as instance variables,
        // then a getter function exists for the player to fetch the data at the end of the waves
        try {
            if (File.Exists(FILE_PATH)) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(FILE_PATH, FileMode.Open);
                Data data = (Data) bf.Deserialize(file);
                file.Close();
                highScore = data.highScore;
                score = data.score;
                wave = Mathf.Max(data.wave - 1, 0);
                level = data.level;
                experience = data.experience;
                health = data.health;
                if (data.ownedProjectileNames != null) {
                    for (int i = 0; i < data.ownedProjectileNames.Length; i++) {
                        ownedProjectiles[data.ownedProjectileNames[i]] = data.ownedProjectileLevels[i];
                    }
                }
                if (data.ownedPassiveNames != null) {
                    for (int i = 0; i < data.ownedPassiveNames.Length; i++) {
                        ownedPassives[data.ownedPassiveNames[i]] = data.ownedPassiveLevels[i];
                    }
                }
                Debug.Log("Loaded data");
            }
        }
        catch (System.Exception e) {
            Debug.LogWarning("Failed to load data, using default values.\n" + e.ToString());
        }
    }

    public void SaveData() {
        try {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(FILE_PATH);
            Data data = new Data();
            data.highScore = highScore;
            data.score = score;
            data.wave = wave;
            data.level = level;
            data.experience = experience;
            data.health = health;
            {
                int size = ownedProjectiles.Count;
                data.ownedProjectileNames = new string[size];
                data.ownedProjectileLevels = new int[size];
                int i = 0;
                foreach (string projectileName in ownedProjectiles.Keys) {
                    data.ownedProjectileNames[i] = projectileName;
                    data.ownedProjectileLevels[i] = ownedProjectiles[projectileName];
                    i++;
                }
            }
            {
                int size = ownedPassives.Count;
                data.ownedPassiveNames = new string[size];
                data.ownedPassiveLevels = new int[size];
                int i = 0;
                foreach(string passiveName in ownedPassives.Keys) {
                    data.ownedPassiveNames[i] = passiveName;
                    data.ownedPassiveLevels[i] = ownedPassives[passiveName];
                    i++;
                }
            }
            bf.Serialize(file, data);
            file.Close();
            Debug.Log("Saved data");
        }
        catch (System.Exception e) {
            Debug.LogWarning("Failed to save data.\n" + e.ToString());
        }
    }

    public void ResetData() {
        Debug.Log("Resetting Data...");
        try {
            LoadDefaultNonPersistent();
            SaveData();
        }
        catch (System.Exception e) {
            Debug.LogWarning("Failed to reset data.\n" + e.ToString());
        }
    }
}

[System.Serializable]
public class Data {
    public int highScore;
    public int score;
    public int wave;
    public int level;
    public int experience;
    public float health;
    public string[] ownedProjectileNames;
    public int[] ownedProjectileLevels;
    public string[] ownedPassiveNames;
    public int[] ownedPassiveLevels;
}
