using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Projectile
{
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void GenerateStats(Transform playerTransform, int index)
    {
        float eulerAngle = playerTransform.eulerAngles.z - stats.spread / 2 + (stats.spread * index / (stats.projectileCount - 1));
        this.angle = (eulerAngle + 90) * Mathf.Deg2Rad;
        transform.eulerAngles = new Vector3(0, 0, eulerAngle);
    }
}
