using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splitter : Projectile
{
    private Spike spike;
    private bool alreadyHit;
    private Player player;

    public override void Start()
    {
        base.Start();

        alreadyHit = false;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        spike = player.spikePrefab;
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
            int toSpawn = getProjectileCount();
            // spawn spike
            for (int i = 0; i < toSpawn; i++) {
                Spike newSpike = Instantiate<Spike>(spike, transform.position, Quaternion.identity, GameObject.FindWithTag("Projectile Folder").transform);
                newSpike.stats = stats;
                newSpike.projectileCount = toSpawn;
                newSpike.GenerateStats(transform, i);
            }

            Destroy(gameObject);
        }
    }

    private int getProjectileCount() {
        int level = player.GetProjectileLevel("Splitter");
        if (level >= 3)
            return 12;
        return 8;
    }
}
