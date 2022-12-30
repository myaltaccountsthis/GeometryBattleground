using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetUpgradeEffect() {
        switch (name) {
            case "Wisdom":
                return "<color=green>+20% Experience</color>";
            case "Power":
                return "<color=green>+10% Damage\n+1 Pierce</color>";
            case "Agility":
                return "<color=green>+1 Walk Speed</color>";
            case "Dexterity":
                return "<color=green>-15% Attack Interval</color>";
        }
        return "";
    }
}
