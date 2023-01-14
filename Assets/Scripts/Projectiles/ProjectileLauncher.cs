using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher
{
    public bool ShouldShoot {
        get => toShoot > 0 && toWait == 0;
    }
    public Projectile Projectile {
        get => projectilePrefab;
    }
    public int ToShoot {
        get => toShoot;
    }

    public bool isSub;

    private Projectile projectilePrefab;
    private Player player;
    private int toWait;
    private int toShoot;
    private ProjectileStats currentStats;

    public ProjectileLauncher(Projectile prefab, Player p) {
        projectilePrefab = prefab;
        player = p;
    }

    // must be called manually because this class does not inherit MonoBehaviour
    public void Update(ProjectileStats stats) {
        toWait--;
        currentStats = stats;
    }

    public void Shoot() {
        isSub = false;
        toShoot = currentStats.projectileCount;
        toWait = 0;
    }

    public void ShootSub() {
        isSub = true;
        toShoot--;
        toWait = currentStats.timeBetweenShots;
    }
}
