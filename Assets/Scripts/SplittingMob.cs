using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplittingMob : Mob
{
    [Tooltip("Mob to spawn when current mob dies")]
    public Mob childMob;
    [Tooltip("The amount of childMobs to spawn on death")]
    public int count;

    private const float OFFSET = .5f;

    public override void onDeath()
    {
        base.onDeath();
        
        if (transform.parent == null)
            return;
            
        // Spawn children on death
        for (int i = 0; i < count; i++) {
            Vector3 offset = new Vector3(Mathf.Cos(2 * Mathf.PI / count) * OFFSET, Mathf.Sin(2 * Mathf.PI / count) * OFFSET, 0);
            Instantiate(childMob, transform.position + offset, transform.rotation, transform.parent);
        }
    }
}
