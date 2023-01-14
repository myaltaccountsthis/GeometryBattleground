using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverMenuHandler : MonoBehaviour
{
    public GameObject gameOverMenu;
    private AudioSource backgroundMusic;
    
    private void Awake()
    {
        backgroundMusic = GameObject.Find("Background Music").GetComponent<AudioSource>();
    }
    
    public void OpenUI()
    {
        if(gameOverMenu.activeInHierarchy) return;
        backgroundMusic.Pause();
        gameOverMenu.SetActive(true);
        GameTime.isPaused = true;
    }

    public void OnTryAgain() {
        CloseUI();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void ToMenu() {
        CloseUI();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
    }

    public void OnExit()
    {
        CloseUI();
        Application.Quit();
    }

    private void CloseUI()
    {
        GameObject.FindWithTag("DataManager").GetComponent<DataManager>().ResetData();
        gameOverMenu.SetActive(false);
        GameTime.isPaused = false;
    }
}