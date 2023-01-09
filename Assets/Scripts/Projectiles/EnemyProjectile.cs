using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : Projectile
{
    public Mob mobToSpawn;
    public Mob rareMobToSpawn;

    private const float PROJECTILE_SPEED = 10f;

    private Map map;
    private bool hitTarget;

    public override void Awake()
    {
        base.Awake();

        map = GameObject.FindWithTag("Map").GetComponent<Map>();
    }

    public override void Start()
    {
        base.Start();

        hitTarget = false;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.GetComponent<Player>() != null) {
            hitTarget = true;
            Destroy(gameObject);
        }
        return;
    }

    void OnDestroy() {
        // only spawn minion if this did not hit the player
        // if statement is probably unnecessary
        if (!hitTarget) {
            Mob toSpawn = mobToSpawn;
            if (Random.value < .1f)
                toSpawn = rareMobToSpawn;
            map.SpawnMob(toSpawn, transform.position);
        }
    }

    public override void GenerateStats(Transform playerTransform, int index = 1)
    {
        this.angle = (playerTransform.eulerAngles.z + 90) * Mathf.Deg2Rad;
        transform.eulerAngles = new Vector3(0, 0, playerTransform.eulerAngles.z);
    }
}
