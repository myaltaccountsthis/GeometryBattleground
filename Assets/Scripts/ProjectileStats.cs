using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class ProjectileStats {
    [Tooltip("Time in between shots, in updates")]
    public int interval { get; }
    [Tooltip("Speed of the projectile, in units per second")]
    public float speed { get; }
    [Tooltip("Drag of the projectile, in units per second^2 (positive drag is negative acceleration)")]
    public float drag { get; }
    [Tooltip("Damage done to mobs by the projectile, in health points (just a unit)")]
    public float damage { get; }
    [Tooltip("Life time of the projectile, in updates")]
    public int lifeTime { get; }
    [Tooltip("Number of mobs the projectile can go through (0 pierce means unlimited)")]
    public int pierce { get; }
    [Tooltip("How many shots should get fired per interval")]
    public int projectileCount { get; }
    [Tooltip("Time in between Projectile Count shots, in updates (does not add on to interval time)")]
    public int timeBetweenShots { get; }
    [Tooltip("Spread of the projectiles (max angle) in degrees")]
    public float spread { get; }
    
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

    public static ProjectileStats Parse(string[] tokens, int startIndex = 0) {
        return new ProjectileStats(Convert.ToInt32(tokens[startIndex]), Convert.ToSingle(tokens[startIndex + 1]), Convert.ToSingle(tokens[startIndex + 2]), Convert.ToSingle(tokens[startIndex + 3]), Convert.ToInt32(tokens[startIndex + 4]), Convert.ToInt32(tokens[startIndex + 5]), Convert.ToInt32(tokens[startIndex + 6]), Convert.ToInt32(tokens[startIndex + 7]), Convert.ToSingle(tokens[startIndex + 8]));
    }

    public static ProjectileStats Empty { get => new ProjectileStats(-1, 0, 0, 0, 0, 0, 0, 0, 0); }
}