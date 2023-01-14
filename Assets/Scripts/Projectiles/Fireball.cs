using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class must have -1 pierce for explosion to work
public class Fireball : Projectile
{
    public Explosion explosion;
    private bool alreadyHit;
    private Player player;
    private const float EXPLOSION_RADIUS = 2;

    public override void Awake()
    {
        base.Awake();
        alreadyHit = false;
    }

    public override void Start()
    {
        base.Start();

        player = GameObject.FindWithTag("Player").GetComponent<Player>();
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

    public override string GetBaseStats(ProjectileStats next)
    {
        string message = base.GetBaseStats(next);
        message += "<color=green>Explosion Size: " + (getExplosionSize(1) * EXPLOSION_RADIUS) + "</color>\n";
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
        if (mob != null && !mob.IsDead && !alreadyHit) {
            alreadyHit = true;
            Destroy(gameObject);
        }
    }

    private void SpawnExplosion() {
        GameObject projectileFolder = GameObject.FindWithTag("Projectile Folder");
        if (projectileFolder == null)
            return;
        Explosion newExplosion = Instantiate<Explosion>(explosion, transform.position, Quaternion.identity, projectileFolder.transform);
        newExplosion.PlaySound();
        newExplosion.LoadStats(stats);
        newExplosion.minSize = .2f;
        newExplosion.maxSize = getExplosionSize();
        newExplosion.explosionLifeTime = 6;
        newExplosion.explosionDecayTime = 20;
    }

    private float getExplosionSize(int level = -1) {
        if (level == -1)
            level = GetLevel(player);
        if (level >= 8)
            return 1.8f;
        if (level >= 5)
            return 1.5f;
        return 1.25f;
    }
}
