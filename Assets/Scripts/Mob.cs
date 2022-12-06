using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mob : MonoBehaviour
{
    public const int DAMAGE_TICKS = 8;
    public static Color DAMAGE_COLOR {
        get => Color.gray;
    }
    [Tooltip("Starting health of the mob, in health points")]
    public float startingHealth;
    [Tooltip("Movement speed of the mob, in units per second")]
    public float movementSpeed;
    [Tooltip("When this mob will start spawning, in updates")]
    public int startingTime;
    [Tooltip("Base damage of this mob to the player before time multipliers, in health points")]
    public float baseDamage;
    [Tooltip("How much experience this mob should drop when killed")]
    public int experienceDrop;

    private float health;
    private int damageTicks;
    private Transform player;
    private Map map;

    void Start()
    {
        health = GetHealth();
        damageTicks = 0;
        player = GameObject.FindWithTag("Player").transform;
        map = GameObject.FindWithTag("Map").GetComponent<Map>();
    }

    // TODO movement and follow player (player has tag "Player")
    void Update()
    {
        if (damageTicks > 0) {
            damageTicks--;
            GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, DAMAGE_COLOR, (float) damageTicks / DAMAGE_TICKS);
        }
        Vector3 playerPos = player.position;
        Vector3 direction = (playerPos - transform.position).normalized;
        transform.position += direction * movementSpeed / 60f;
    }

    // Makes this mob take damage
    public void TakeDamage(Projectile projectile) {
        health -= projectile.stats.damage;
        if (health <= 0) {
            map.InstantiateExperience(experienceDrop, transform.position);
            Destroy(gameObject);
        }
        GetComponent<SpriteRenderer>().color = DAMAGE_COLOR;
        damageTicks = DAMAGE_TICKS;
    }

    // Returns the damage this mob should deal after some time
    public float GetDamage() {
        int updates = Player.Updates;
        if (updates < 18000)
            return baseDamage * (1 + updates / 12000f);
        return baseDamage * (10 - 10 * Mathf.Pow(3, -updates / 60000f));
    }

    // Returns the health that this mob should spawn with after some time
    public float GetHealth() {
        return startingHealth * (Mathf.Pow(Player.Updates, 1.2f) / 40000f + 1);
    }
}
