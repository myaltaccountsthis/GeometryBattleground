using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectileStats {
    [Tooltip("Time in between shots, in updates")]
    public int interval;
    [Tooltip("Speed of the projectile, in units per second")]
    public float speed;
    [Tooltip("Drag of the projectile, in units per second^2 (positive drag is negative acceleration)")]
    public float drag;
    [Tooltip("Damage done to mobs by the projectile, in health points (just a unit)")]
    public float damage;
    [Tooltip("Life time of the projectile, in updates")]
    public int lifeTime;
    [Tooltip("Number of mobs the projectile can go through (0 pierce means unlimited)")]
    public int pierce;
    [Tooltip("How many shots should get fired per interval")]
    public int projectileCount;
    [Tooltip("Time in between Projectile Count shots, in updates (does not add on to interval time)")]
    public int timeBetweenShots;
    [Tooltip("Spread of the projectiles (max angle) in degrees")]
    public float spread;
    
    public ProjectileStats(int interval, float speed, float drag, float damage, int lifeTime, int pierce, int projectileCount, int timeBetweenShots, float spread) {
        this.interval = interval;
        this.speed = speed;
        this.drag = drag;
        this.damage = damage;
        this.lifeTime = lifeTime;
        this.pierce = pierce;
        this.projectileCount = projectileCount;
        this.timeBetweenShots = timeBetweenShots;
        this.spread = spread;
    }
}