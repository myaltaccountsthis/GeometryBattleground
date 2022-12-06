using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// there will be instances of each sub-class in Player's projectiles instance variable. those will Instantiate<>() new projectiles
public abstract class Projectile : MonoBehaviour
{
    public ProjectileStats stats;
    // in radians
    [HideInInspector]
    public float angle;
    [HideInInspector]
    public int toShoot;
    [HideInInspector]
    public int toWait;

    private Collider2D collision;
    private int creationTime;
    private int pierceLeft;

    public virtual void Start()
    {
        collision = GetComponent<Collider2D>();
        Debug.Assert(collision.isTrigger, "Error: this projectile's Collider2D property isTrigger is false");
        creationTime = Player.Updates;
        pierceLeft = stats.pierce;
    }

    public virtual void Update()
    {
        if (Player.Updates >= creationTime + stats.lifeTime) {
            Destroy(gameObject);
        }
        
        float currentSpeed = Mathf.Max((stats.speed - stats.drag * (Player.Updates - creationTime) / 60f) / 60f, 0f);
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
