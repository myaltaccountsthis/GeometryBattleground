using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class must have -1 pierce for explosion to work
public class Fireball : Projectile
{
    private Explosion explosion;
    private bool alreadyHit;
    private Player player;

    public override void Awake()
    {
        base.Awake();
        alreadyHit = false;
    }

    public override void Start()
    {
        base.Start();

        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        explosion = GameObject.FindWithTag("Player").GetComponent<Player>().explosionPrefab;
    }

    public override string GetUpgradeEffect(int level, ProjectileStats next) {
        string message = base.GetUpgradeEffect(level, next);
        float size = getExplosionSize(level);
        if (getExplosionSize(level + 1) != size) {
            float dSize = getExplosionSize(level + 1) - size;
            message += (dSize > 0 ? "<color=green>+" : "<color=red>-") + ProjectileStats.Round(Mathf.Abs(100f * dSize / size), 1) + "% Size</color>\n";
        }
        return message;
    }

    public override void GenerateStats(Transform playerTransform, int index = 1)
    {
        this.angle = (playerTransform.eulerAngles.z + 90) * Mathf.Deg2Rad;
        transform.eulerAngles = new Vector3(0, 0, playerTransform.eulerAngles.z);
    }

    void OnDestroy()
    {
        SpawnExplosion();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Mob mob = collider.GetComponent<Mob>();
        if (mob != null && !alreadyHit) {
            alreadyHit = true;
            Destroy(gameObject);
        }
    }

    void SpawnExplosion() {
        Explosion newExplosion = Instantiate<Explosion>(explosion, transform.position, Quaternion.identity, GameObject.FindWithTag("Projectile Folder").transform);
        newExplosion.LoadStats(stats);
        newExplosion.minSize = .2f;
        newExplosion.maxSize = getExplosionSize();
        newExplosion.explosionLifeTime = 6;
        newExplosion.explosionDecayTime = 20;
    }

    private float getExplosionSize(int level = -1) {
        if (level == -1)
            level = player.GetProjectileLevel("Fireball");
        if (level >= 8)
            return 1.5f;
        if (level >= 5)
            return 1.2f;
        return 1f;
    }
}
