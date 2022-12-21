using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Projectile
{
    private const float offset = .5f;

    public override void GenerateStats(Transform playerTransform, int index)
    {
        float eulerAngle = playerTransform.eulerAngles.z;
        this.angle = (eulerAngle + 90) * Mathf.Deg2Rad;
        transform.eulerAngles = new Vector3(0, 0, eulerAngle);
        if (stats.projectileCount > 1) {
            Vector3 direction = new Vector3(Mathf.Cos(angle + Mathf.PI / 2), Mathf.Sin(angle + Mathf.PI / 2), 0);
            transform.position += direction * offset * (-.5f + (float)index / Mathf.Max(stats.projectileCount - 1, 1));
        }
    }
}
