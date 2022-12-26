using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Drop
{
    public override void PickUp(Player player)
    {
        base.PickUp(player);
        player.CollectHealth(this);
    }
}
