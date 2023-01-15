using System.Collections;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverMenuHandler : MonoBehaviour
{
    public GameObject gameOverMenu;

    private AudioSource backgroundMusic;
    private AudioSource darkAura;
    private DataManager dataManager;
    private TransitionManager transitionManager;
    
    private void Awake()
    {
        backgroundMusic = GameObject.Find("Background Music").GetComponent<AudioSource>();
        darkAura = GameObject.Find("Dark Aura").GetComponent<AudioSource>();
        dataManager = GameObject.FindWithTag("DataManager").GetComponent<DataManager>();
        transitionManager = GameObject.FindWithTag("TransitionManager").GetComponent<TransitionManager>();
    }
    
    public void OpenUI()
    {
        if(gameOverMenu.activeInHierarchy) return;
        dataManager.ResetData();
        backgroundMusic.Pause();
        if(darkAura.isPlaying) darkAura.Pause();
        gameOverMenu.SetActive(true);
        GameTime.isPaused = true;
    }

    public void OnTryAgain() {
        transitionManager.ChangeScene("Game");
    }

    public void ToMenu() {
        transitionManager.ChangeScene("Home");
    }

    public void OnExit()
    {
        Application.Quit();
    }
}