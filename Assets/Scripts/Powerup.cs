using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : Drop
{
    public string type;
    public int duration;

    public Sprite sprite {
        get => GetComponent<SpriteRenderer>().sprite;
    }

    public override void PickUp(Player player)
    {
        base.PickUp(player);
        player.CollectPowerup(this);
    }
}
