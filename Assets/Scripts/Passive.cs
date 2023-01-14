using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Passive : MonoBehaviour, IUpgradeable
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // Get the numerical level of the passive
    public int GetLevel(Player player)
    {
        return player.GetPassiveLevel(name);
    }
    // Get the level to display
    public string GetLevelText(int nextLevel) {
        return "Level " + nextLevel;
    }
    // Get the name of the passive
    public string GetName()
    {
        return name;
    }
    // Get the change in stats between levels using the player
    public string GetUpgradeEffect(Player player)
    {
        return GetUpgradeEffect(GetLevel(player));
    }
    // Get the change in stats between levels
    public string GetUpgradeEffect(int level) {
        switch (name) {
            case "Wisdom":
                return "<color=green>+20% Experience</color>";
            case "Power":
                return "<color=green>+10% Damage\n" + (level % 2 == 0 ? "+1 Pierce" : "") + "</color>";
            case "Agility":
                return "<color=green>+1 Walk Speed</color>";
            case "Dexterity":
                return "<color=green>-15% Attack Interval</color>";
        }
        return "";
    }
    // Get the passive's sprite for upgrade image
    public Sprite GetSprite() {
        return GetComponent<Image>().sprite;
    }
}
