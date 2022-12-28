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
    [Tooltip("When this mob will start spawning")]
    public int startingWave;
    [Tooltip("Base damage of this mob to the player before time multipliers, in health points")]
    public float baseDamage;
    [Tooltip("How much experience this mob should drop when killed")]
    public int experienceDrop;
    [Tooltip("Percent that experience will be dropped when killing this mob")]
    public float experiencePercent;
    [Tooltip("How much this mob should be weighted into spawning (higher weight = greater chance of spawning)")]
    public int spawnWeight;
    [Tooltip("How many points this mob awards you when defeated")]
    public int score;

    private float health;
    private int damageTicks;
    private Player player;
    private Map map;
    private SpriteRenderer spriteRenderer;
    
    private const float HEALTH_CHANCE = .003f;
    // private const float POWERUP_CHANCE = .2f;
    private const float POWERUP_CHANCE = .005f;

    void Start()
    {
        damageTicks = 0;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        map = GameObject.FindWithTag("Map").GetComponent<Map>();
        health = GetHealth();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // follow player (player has tag "Player")
    void Update()
    {
        if (GameTime.isPaused)
            return;

        if (damageTicks > 0) {
            damageTicks--;
            spriteRenderer.color = Color.Lerp(Color.white, DAMAGE_COLOR, (float) damageTicks / DAMAGE_TICKS);
        }
        
        Vector3 playerPos = player.transform.position;
        Vector3 direction = (playerPos - transform.position).normalized;
        transform.position += direction * movementSpeed / 60f;
        transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x) - 90);
    }

    // Makes this mob take damage
    public void TakeDamage(Projectile projectile) {
        float damage = projectile.stats.damage;
        if (player.IsPowerupActive("Double Damage"))
            damage *= 2;
        health -= damage;
        if (health <= 0) {
            player.AddScore(score);
            if (Random.value < POWERUP_CHANCE * experiencePercent * experienceDrop)
                map.InstantiatePowerup(transform.position);
            else if (Random.value < HEALTH_CHANCE * experiencePercent * experienceDrop)
                map.InstantiateHealth(transform.position);
            else if (Random.value < experiencePercent)
                map.InstantiateExperience(experienceDrop, transform.position);
            Destroy(gameObject);
        }
        spriteRenderer.color = DAMAGE_COLOR;
        damageTicks = DAMAGE_TICKS;
    }

    // Returns the damage this mob should deal after some time
    public float GetDamage() {
        return baseDamage * (Mathf.Pow(player.Wave, 1.3f) / 40 + 1);
    }

    // Returns the health that this mob should spawn with after some time
    public float GetHealth() {
        return startingHealth * (Mathf.Pow(player.Wave, 1.3f) / 20 + 1);
    }
}
