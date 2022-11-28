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

    private float health;
    private int damageTicks;

    void Start()
    {
        health = GetHealth();
        damageTicks = 0;
    }

    // TODO movement and follow player (player has tag "Player")
    void Update()
    {
        if (damageTicks > 0) {
            damageTicks--;
            GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, DAMAGE_COLOR, (float) damageTicks / DAMAGE_TICKS);
        }
        Vector3 playerPos = GameObject.FindWithTag("Player").transform.position;
        Vector3 direction = (playerPos - transform.position).normalized;
        transform.position += direction * movementSpeed / 60f;
    }

    // Makes this mob take damage
    public void TakeDamage(Projectile projectile) {
        health -= projectile.damage;
        if (health <= 0) {
            Destroy(gameObject);
        }
        GetComponent<SpriteRenderer>().color = DAMAGE_COLOR;
        damageTicks = DAMAGE_TICKS;
    }

    // Returns the damage this mob should deal after some time
    private float GetDamage() {
        int updates = Player.Updates;
        if (updates < 300)
            return baseDamage * (1 + updates / 12000f);
        return baseDamage * (10 - 10 * Mathf.Pow(3, -updates / 60000f));
    }

    // Returns the health that this mob should spawn with after some time
    private float GetHealth() {
        return startingHealth * (Mathf.Pow(Player.Updates, 1.2f) / 40000f + 1);
    }
}
