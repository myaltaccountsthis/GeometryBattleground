using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void Update() {
        if (Input.GetAxis("Submit") > 0) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
}
