using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INSTANCE VARIABLES MUST BE INSTANTIATED BY SPLITTER
public class Spike : Projectile
{
    public int projectileCount;

    public override void GenerateStats(Transform playerTransform, int index = 1)
    {
        float eulerAngle = index * 360f / projectileCount;
        this.angle = (eulerAngle + 90) * Mathf.Deg2Rad;
        transform.eulerAngles = new Vector3(0, 0, eulerAngle);
        stats.lifeTime = 30;
    }
}
