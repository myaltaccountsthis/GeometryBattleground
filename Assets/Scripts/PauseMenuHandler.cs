using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class ClickEvent : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Action onClick;
    private Image image;
    private Color originalColor;

    void Start() {
        image = GetComponent<Image>();
        originalColor = image.color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        image.color = originalColor;
        onClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = Color.Lerp(originalColor, Color.black, .1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = originalColor;
    }

    public void SetClickFunction(Action action) {
        onClick = action;
    }
}

public class PauseMenuHandler : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject resumeButton;
    public GameObject resetButton;
    public GameObject exitButton;

    private bool prevPressed;

    void Start() {
        ClickEvent clickEvent = resumeButton.AddComponent<ClickEvent>();
        clickEvent.SetClickFunction(CloseUI);
        clickEvent = resetButton.AddComponent<ClickEvent>();
        clickEvent.SetClickFunction(() => {
            GameTime.isPaused = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        });
        clickEvent = exitButton.AddComponent<ClickEvent>();
        clickEvent.SetClickFunction(() => Application.Quit());
    }

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
}
