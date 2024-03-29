using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public struct ProjectileStats {
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

    public string GetBaseUpgradeEffect(ProjectileStats next) {
        string message = "";
        if (next.interval != interval) {
            int dInterval = next.interval - interval;
            message += (dInterval > 0 ? "<color=red>+" : "<color=green>-") + Round(Math.Abs(100f * dInterval / interval), 1) + "% Attack Interval</color>\n";
        }
        if (next.damage != damage) {
            float dDamage = next.damage - damage;
            message += (dDamage > 0 ? "<color=green>+" : "<color=red>-") + Round(Math.Abs(100f * dDamage / damage), 1) + "% Damage</color>\n";
        }
        if (next.pierce != pierce) {
            int dPierce = next.pierce - pierce;
            message += (dPierce > 0 ? "<color=green>+" : "<color=red>-") + Mathf.Abs(dPierce) + " Pierce</color>\n";
        }
        if (next.projectileCount != projectileCount) {
            int dProjectileCount = next.projectileCount - projectileCount;
            message += (dProjectileCount > 0 ? "<color=green>+" : "<color=red>-") + Mathf.Abs(dProjectileCount) + " Projectile" + (dProjectileCount == 1 ? "" : "s") + "</color>\n";
        }
        return message;
    }

    public string GetBaseStats() {
        string message = "";
        message += "<color=green>Attack Interval: " + interval + "</color>\n";
        message += "<color=green>Damage: " + damage + "</color>\n";
        message += "<color=green>Pierce: " + (pierce == -1 ? "Infinite" : pierce) + "</color>\n";
        if (projectileCount > 1)
            message += "<color=green>Projectiles: " + projectileCount + "</color>\n";
        return message;
    }

    // Increase projectile stats depending on passives
    public void ApplyPassives(Dictionary<string, int> passives) {
        foreach(string passiveName in passives.Keys) {
            int passiveLevel = passives[passiveName];
            switch (passiveName) {
                case "Dexterity":
                    // Increase damage instead if attack can't be faster
                    if (interval == 1)
                        damage *= (1 + .15f * passiveLevel);
                    else {
                        interval = Mathf.CeilToInt(interval / (1 + .15f * passiveLevel));
                        timeBetweenShots = Mathf.CeilToInt(timeBetweenShots / (1 + .15f * passiveLevel));
                    }
                    break;
                case "Power":
                    damage *= (1 + passiveLevel * .2f);
                    if (pierce != -1)
                        pierce += (passiveLevel + 1) / 2;
                    break;
            }
        }
    }

    public static float Round(float num, int digits) {
        return Mathf.Round(num * Mathf.Pow(10, digits)) / Mathf.Pow(10, digits);
    }

    public static ProjectileStats Parse(string[] tokens, int startIndex = 0) {
        return new ProjectileStats(Convert.ToInt32(tokens[startIndex]), Convert.ToSingle(tokens[startIndex + 1]), Convert.ToSingle(tokens[startIndex + 2]), Convert.ToSingle(tokens[startIndex + 3]), Convert.ToInt32(tokens[startIndex + 4]), Convert.ToInt32(tokens[startIndex + 5]), Convert.ToInt32(tokens[startIndex + 6]), Convert.ToInt32(tokens[startIndex + 7]), Convert.ToSingle(tokens[startIndex + 8]));
    }

    private static ProjectileStats emptyStats = new ProjectileStats(-1, 0, 0, 0, 0, 0, 0, 0, 0);
    public static ProjectileStats Empty {
        get {
            return emptyStats;
        }
    }
}