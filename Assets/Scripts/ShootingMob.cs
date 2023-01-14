using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingMob : Mob
{
    public EnemyProjectile projectilePrefab;

    private const int SHOOT_COOLDOWN = 420;
    private const int SHOOT_DURATION = 180;
    private const int SHOOT_INTERVAL = 30;
    // time before first shot is fired
    private const int SHOOT_FORESWING = 60;
    private const int SHOOT_AMOUNT = 3;

    private float originalMovementSpeed;
    private ProjectileStats projectileStats;
    private Transform projectileFolder;
    private int shooting;
    private int ammo;
    private int cooldown;

    void Awake() {
        originalMovementSpeed = movementSpeed;
        projectileStats = new ProjectileStats(0, 10, 0, 30, 75, 1, 1, 0, 0);
        projectileFolder = GameObject.FindWithTag("Projectile Folder").transform;
    }

    public override void Start() {
        base.Start();

        shooting = 0;
        ammo = 0;
        cooldown = SHOOT_COOLDOWN;
    }

    public override void Update() {
        base.Update();

        if (GameTime.isPaused || IsDead)
            return;

        float distance = (player.transform.position - transform.position).magnitude;
        if (distance <= 8 && cooldown <= 0) {
            // start shooting sequence
            shooting = SHOOT_DURATION;
            cooldown = SHOOT_COOLDOWN;
            ammo = SHOOT_AMOUNT;
            movementSpeed = 0;
        }
        cooldown--;
        if (shooting > 0) {
            if (SHOOT_DURATION - shooting - SHOOT_FORESWING > 0 && (SHOOT_DURATION - shooting - SHOOT_FORESWING) % SHOOT_INTERVAL == 0 && ammo > 0)
                Shoot();
            shooting--;
            if (shooting == 0) {
                movementSpeed = originalMovementSpeed;
            }
        }
    }

    public void Shoot() {
        ammo--;
        EnemyProjectile enemyProjectile = Instantiate<EnemyProjectile>(projectilePrefab, transform.position, Quaternion.identity, projectileFolder);
        enemyProjectile.LoadStats(projectileStats);
        enemyProjectile.GenerateStats(transform);
    }
}
