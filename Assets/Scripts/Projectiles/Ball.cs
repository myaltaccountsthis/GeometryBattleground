using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : Projectile
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

    public override int GetCount()
    {
        return 1;
    }

    public override void GenerateStats(float angle)
    {
        this.angle = angle;
    }
}
