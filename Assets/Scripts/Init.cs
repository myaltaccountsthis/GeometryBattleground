using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    private TransitionManager transitionManager;
    private DataManager dataManager;

    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        dataManager = GameObject.FindWithTag("DataManager").GetComponent<DataManager>();
        transitionManager = GameObject.FindWithTag("TransitionManager").GetComponent<TransitionManager>();
    }

    void Update() {
        if (Input.GetAxis("Submit") > 0) {
            ChangeScene();
        }
    }

    public void ChangeScene() {
        transitionManager.ChangeScene("Game");
    }

    public void ResetAndChangeScene() {
        dataManager.ResetData();
        ChangeScene();
    }

    public void Exit() {
        Application.Quit();
    }
}
