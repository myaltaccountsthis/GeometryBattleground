using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupUI : MonoBehaviour
{
    public Image backgroundImage;
    public Image cooldownImage;
    private RectTransform rectTransform;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        cooldownImage.fillAmount = 1f;
    }

    public void UpdateCooldown(float ratio) {
        cooldownImage.fillAmount = ratio;
    }

    public void SetLocation(int index) {
        rectTransform.localPosition = new Vector3(index * -100, 0);
    }

    public void LoadImage(Powerup powerup) {
        Sprite sprite = powerup.sprite;
        backgroundImage.sprite = sprite;
        cooldownImage.sprite = sprite;
    }
}
