using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuHandler : MonoBehaviour
{
    public GameObject pauseMenu;

    private bool prevPressed;
    
    void Update() {
        bool pressed = Input.GetAxis("Cancel") > 0;
        if (pressed && !prevPressed) {
            ToggleUI();
        }
        prevPressed = pressed;
    }

    private void CloseUI() {
        pauseMenu.SetActive(false);
        GameTime.isPaused = false;
    }

    public void ToggleUI() {
        if (pauseMenu.activeInHierarchy) {
            CloseUI();
        }
        // if any other thing that pauses the game isnt active
        else if (!GameTime.isPaused) {
            // open ui
            pauseMenu.SetActive(true);
            GameTime.isPaused = true;
        }
    }

    public void OnResume() {
        CloseUI();
    }

    public void ToMenu() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
    }

    public void OnExit() {
        Application.Quit();
    }
}
