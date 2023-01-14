using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HudUI : MonoBehaviour
{
    public RectTransform healthBar;
    public RectTransform shieldBar;
    public RectTransform expBar;
    public RectTransform powerupHud;
    public PowerupUI powerupPrefab;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    private Dictionary<string, PowerupUI> activePowerupUI;
    private Map map;

    void Awake() {
        activePowerupUI = new Dictionary<string, PowerupUI>();
    }

    void Start() {
        map = GameObject.FindWithTag("Map").GetComponent<Map>();
    }

    public void UpdateHealth(float health, float totalHealth, float shield) {
        float yScale = shield > 0 ? .5f : 1f;
        healthBar.localScale = new Vector3(health / totalHealth, yScale, 1);
        shieldBar.localScale = new Vector3(shield / totalHealth, yScale, 1);
    }

    public void UpdateExpBar(int experience, int nextExperience, int level) {
        expBar.localScale = new Vector3(Mathf.Min(1f, (float)experience / nextExperience), 1, 1);
        levelText.text = level + "";
    }

    public void UpdateScore(int score) {
        scoreText.text = score + "";
    }
    // Update the powerup UI to keep up the duration
    public void UpdatePowerups(Dictionary<string, int> activePowerups) {
        // remove inactive powerups
        List<string> activePowerupNames = new List<string>(activePowerupUI.Keys);
        foreach (string powerupName in activePowerupNames) {
            if (!activePowerups.ContainsKey(powerupName)) {
                Destroy(activePowerupUI[powerupName].gameObject);
                activePowerupUI.Remove(powerupName);
            }
        }
        // add new powerups and reposition all
        int i = 0;
        foreach (string powerupName in activePowerups.Keys) {
            Powerup originalPowerup = map.GetPowerup(powerupName);
            if (!activePowerupUI.ContainsKey(powerupName)) {
                PowerupUI newPowerupUI = Instantiate<PowerupUI>(powerupPrefab, powerupHud);
                newPowerupUI.LoadImage(originalPowerup);
                activePowerupUI.Add(powerupName, newPowerupUI);
            }
            PowerupUI powerupUI = activePowerupUI[powerupName];
            powerupUI.UpdateCooldown((float) activePowerups[powerupName] / originalPowerup.duration);
            powerupUI.SetLocation(i);
            i++;
        }
    }
}
