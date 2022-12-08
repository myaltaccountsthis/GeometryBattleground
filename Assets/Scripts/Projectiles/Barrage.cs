using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrage : Projectile
{
    public override void GenerateStats(Transform playerTransform, int index)
    {
        float eulerAngle = playerTransform.eulerAngles.z + Random.Range(-.5f, .5f) * stats.spread;
        this.angle = (eulerAngle + 90) * Mathf.Deg2Rad;
        transform.eulerAngles = new Vector3(0, 0, eulerAngle);
    }
}
