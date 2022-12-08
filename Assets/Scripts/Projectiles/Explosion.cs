using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : Projectile
{
    public float minSize;
    public float maxSize;
    public int explosionLifeTime;

    private Vector3 startPosition;
    private int aliveFor;

    public override void Start()
    {
        base.Start();

        startPosition = transform.position;
        aliveFor = 0;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        aliveFor++;
        float scale = (maxSize - minSize) * aliveFor / explosionLifeTime + minSize;
        transform.localScale = new Vector3(scale, scale, 1);
        transform.position = startPosition;

        if (aliveFor >= explosionLifeTime)
            Destroy(gameObject);
    }

    public override void GenerateStats(Transform playerTransform, int index = 1)
    {
        // things that create an explosion will just give their stats to this
    }
}
