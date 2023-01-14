using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public DataManager dataManager;
    public TextMeshProUGUI scoreText;

    void Awake() {
        dataManager = GameObject.FindWithTag("DataManager").GetComponent<DataManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        scoreText.text = "" + dataManager.highScore;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
