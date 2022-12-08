using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : Projectile
{
    public override void GenerateStats(Transform playerTransform, int index)
    {
        this.angle = (playerTransform.eulerAngles.z + 90) * Mathf.Deg2Rad;
    }
}
