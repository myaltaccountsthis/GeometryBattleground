using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkAura : Projectile
{
    public override void GenerateStats(Transform playerTransform, int index = 1)
    {
        transform.localScale = new Vector3(stats.speed, stats.speed, 1);
        transform.eulerAngles = new Vector3(0, 0, Player.Updates * 1f);
    }
}
