using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class DataManager : MonoBehaviour
{
    public int highScore;

    private string FILE_PATH;

    void Awake() {
        FILE_PATH = Application.persistentDataPath + "/gamedata.dat";
        LoadData();
    }

    void OnDestroy() {
        SaveData();
    }

    void LoadData() {
        if (File.Exists(FILE_PATH)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(FILE_PATH, FileMode.Open);
            Data data = (Data) bf.Deserialize(file);
            file.Close();
            highScore = data.highScore;
            Debug.Log("Loaded data");
        }
        else {
            highScore = 0;
        }
    }

    public void SaveData() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(FILE_PATH);
        Data data = new Data();
        data.highScore = highScore;
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Saved data");
    }
}

[System.Serializable]
class Data {
    public int highScore;
}
