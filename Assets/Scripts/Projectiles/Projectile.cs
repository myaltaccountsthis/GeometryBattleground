using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// there will be instances of each sub-class in Player's projectiles instance variable. those will Instantiate<>() new projectiles
public abstract class Projectile : MonoBehaviour
{
    [Tooltip("Time in between shots, in updates")]
    public int interval;
    [Tooltip("Speed of the projectile, in units per second")]
    public float speed;
    [Tooltip("Drag of the projectile, in units per second^2 (positive drag is negative acceleration)")]
    public float drag;
    [Tooltip("Damage done to mobs by the projectile, in health points (just a unit)")]
    public float damage;
    [Tooltip("Life time of the projectile, in updates")]
    public int lifeTime;
    [Tooltip("Number of mobs the projectile can go through (0 pierce means unlimited)")]
    public int pierce;
    [Tooltip("How many shots should get fired per interval")]
    public int projectileCount;
    // in radians
    [HideInInspector]
    public float angle;

    private Collider2D collision;
    private int creationTime;
    private int pierceLeft;

    public virtual void Start()
    {
        collision = GetComponent<Collider2D>();
        Debug.Assert(collision.isTrigger, "Error: this projectile's Collider2D property isTrigger is false");
        creationTime = Player.Updates;
        pierceLeft = pierce;
    }

    public virtual void Update()
    {
        if (Player.Updates >= creationTime + lifeTime) {
            Destroy(gameObject);
        }
        
        float currentSpeed = (speed - drag * (Player.Updates - creationTime)) / 60f;
        transform.position += new Vector3(Mathf.Cos(angle) * currentSpeed, Mathf.Sin(angle) * currentSpeed, 0);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Mob mob = collider.GetComponent<Mob>();
        if (mob != null) {
            if (pierceLeft == 0)
                return;
            pierceLeft--;
            mob.TakeDamage(this);
            if (pierceLeft == 0)
                Destroy(gameObject);
        }
    }

    // Generate random instance variable values for projectile
    public abstract void GenerateStats(Transform playerTransform, int index = 1);
}
