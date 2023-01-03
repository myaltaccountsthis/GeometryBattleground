using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Drop
{
    public const int SCORE = 100;
    public override void PickUp(Player player)
    {
        base.PickUp(player);
        player.CollectHealth(this);
    }
}
