using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : Drop
{
    public string type;
    public int duration;

    public override void PickUp(Player player)
    {
        base.PickUp(player);
        player.CollectPowerup(this);
    }
}
