using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splitter : Projectile
{
    public Spike spike;
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
    }

    public override string GetUpgradeEffect(int level, ProjectileStats next) {
        string message = base.GetUpgradeEffect(level, next);
        int count = GetProjectileCount(level);
        if (GetProjectileCount(level + 1) != count) {
            int dCount = GetProjectileCount(level + 1) - count;
            message += (dCount > 0 ? "<color=green>+" : "<color=red>-") + Mathf.Abs(dCount) + " Spikes</color>\n";
        }
        return message;
    }

    public override void GenerateStats(Transform playerTransform, int index)
    {
        this.angle = (playerTransform.eulerAngles.z + 90) * Mathf.Deg2Rad;
        transform.eulerAngles = new Vector3(0, 0, playerTransform.eulerAngles.z);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Mob mob = collider.GetComponent<Mob>();
        if (mob != null && !alreadyHit) {
            alreadyHit = true;
            Destroy(gameObject);
        }
    }

    void SpawnSpikes() {
        int toSpawn = GetProjectileCount();
        for (int i = 0; i < toSpawn; i++) {
            Spike newSpike = Instantiate<Spike>(spike, transform.position, Quaternion.identity, GameObject.FindWithTag("Projectile Folder").transform);
            newSpike.LoadStats(stats);
            newSpike.projectileCount = toSpawn;
            newSpike.GenerateStats(transform, i);
        }
    }

    void OnDestroy()
    {
        SpawnSpikes();
    }

    public int GetProjectileCount(int level = -1) {
        if (level == -1)
            level = player.GetProjectileLevel("Splitter");
        if (level >= 6)
            return 16;
        if (level >= 3)
            return 12;
        return 8;
    }
}
