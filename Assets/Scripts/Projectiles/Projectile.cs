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
    protected int creationTime;
    private int pierceLeft;
    private BoundsInt bounds;

    public virtual void Start()
    {
        collision = GetComponent<Collider2D>();
        bounds = GameObject.FindWithTag("Map").GetComponent<Map>().Bounds;
        Debug.Assert(collision.isTrigger, "Error: this projectile's Collider2D property isTrigger is false");
        creationTime = Player.Updates;
        pierceLeft = stats.pierce;
    }

    public virtual void Update()
    {
        if (GameTime.isPaused)
            return;

        if (Player.Updates >= creationTime + stats.lifeTime || !bounds.Contains(Vector3Int.FloorToInt(transform.position))) {
            Destroy(gameObject);
        }
        
        float currentSpeed = Mathf.Max((stats.speed - stats.drag * (Player.Updates - creationTime) / 60f) / 60f, 0f);
        transform.position += new Vector3(Mathf.Cos(angle) * currentSpeed, Mathf.Sin(angle) * currentSpeed, 0);
    }

    public virtual void ProjectileUpdate() {

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
    // Load stats from the ones read from a text file
    public void LoadStats(ProjectileStats stats) {
        this.stats = stats;
    }
}
