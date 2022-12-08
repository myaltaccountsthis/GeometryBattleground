using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class must have -1 pierce for explosion to work
public class Fireball : Projectile
{
    private Explosion explosion;
    private bool alreadyHit;

    public override void Start()
    {
        base.Start();

        alreadyHit = false;
        explosion = GameObject.FindWithTag("Player").GetComponent<Player>().explosionPrefab;
    }

    public override void GenerateStats(Transform playerTransform, int index = 1)
    {
        this.angle = (playerTransform.eulerAngles.z + 90) * Mathf.Deg2Rad;
        transform.eulerAngles = new Vector3(0, 0, playerTransform.eulerAngles.z);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Mob mob = collider.GetComponent<Mob>();
        if (mob != null && !alreadyHit) {
            alreadyHit = true;
            // spawn explosion at this spot
            Explosion newExplosion = Instantiate<Explosion>(explosion, transform.position, Quaternion.identity, GameObject.FindWithTag("Projectile Folder").transform);
            newExplosion.stats = stats;
            newExplosion.minSize = .2f;
            newExplosion.maxSize = 1f;
            newExplosion.explosionLifeTime = 30;

            Destroy(gameObject);
        }
    }
}
