using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : Projectile
{
    public Mob mobToSpawn;
    public Mob rareMobToSpawn;
    public EnemyExplosion explosion;

    private const float PROJECTILE_SPEED = 10f;

    private Map map;
    private bool hitTarget;
    private ProjectileStats explosionStats;

    public override void Awake()
    {
        base.Awake();

        map = GameObject.FindWithTag("Map").GetComponent<Map>();
        explosionStats = new ProjectileStats(0, 0, 0, 30, 6, -1, 1, 0, 0);
    }

    public override void Start()
    {
        base.Start();

        hitTarget = false;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.GetComponent<Player>() != null) {
            hitTarget = true;
            SpawnExplosion();
            Destroy(gameObject);
        }
    }

    void OnDestroy() {
        // only spawn minion if this did not hit the player
        // if statement is probably unnecessary
        if (!hitTarget) {
            Mob toSpawn = mobToSpawn;
            if (Random.value < .1f)
                toSpawn = rareMobToSpawn;
            map.SpawnMob(toSpawn, transform.position);
            SpawnExplosion();
        }
    }

    private void SpawnExplosion() {
        EnemyExplosion newExplosion = Instantiate<EnemyExplosion>(explosion, transform.position, Quaternion.identity, GameObject.FindWithTag("Projectile Folder").transform);
        newExplosion.LoadStats(explosionStats);
        newExplosion.minSize = .2f;
        newExplosion.maxSize = EnemyExplosion.MAX_SIZE;
        newExplosion.explosionLifeTime = 6;
        newExplosion.explosionDecayTime = 20;
    }

    public override void GenerateStats(Transform playerTransform, int index = 1)
    {
        this.angle = (playerTransform.eulerAngles.z + 90) * Mathf.Deg2Rad;
        transform.eulerAngles = new Vector3(0, 0, playerTransform.eulerAngles.z);
    }
}
