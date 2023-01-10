using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experience : Drop
{
    public int Value {
        get => experienceValue;
    }

    public bool isNatural;

    private int experienceValue;

    public override void PickUp(Player player) {
        base.PickUp(player);
        player.CollectExp(this);
    }

    public void SetExperience(int experience) {
        experienceValue = experience;
    }
}
