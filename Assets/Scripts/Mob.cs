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

    public ulong audioDelay;
    
    private float health;
    private bool dead;
    private int damageTicks;
    protected Player player;
    private Map map;
    private SpriteRenderer spriteRenderer;
    protected AudioSource audioSrc;
    
    private const float HEALTH_CHANCE = .04f;
    // private const float POWERUP_CHANCE = 1f;
    private const float POWERUP_CHANCE = .03f;

    public virtual void Start()
    {
        damageTicks = 0;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        map = GameObject.FindWithTag("Map").GetComponent<Map>();
        health = GetHealth();
        dead = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSrc = GetComponent<AudioSource>();
    }

    // follow player (player has tag "Player")
    public virtual void Update()
    {
        if (GameTime.isPaused || dead)
            return;

        if (damageTicks > 0) {
            damageTicks--;
            spriteRenderer.color = Color.Lerp(Color.white, DAMAGE_COLOR, (float) damageTicks / DAMAGE_TICKS);
        }
        
        Vector3 playerPos = player.transform.position;
        Vector3 direction = (playerPos - transform.position).normalized;
        transform.position += direction * GetSpeed() / 60f;
        transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x) - 90);
    }

    // Makes this mob take damage
    public void TakeDamage(Projectile projectile) {
        PlaySound();
        float damage = projectile.stats.damage;
        if (player.IsPowerupActive("Double Damage"))
            damage *= 2;
        health -= damage;
        if (health <= 0 && !dead) {
            dead = true;
            onDeath();
        }
        spriteRenderer.color = DAMAGE_COLOR;
        damageTicks = DAMAGE_TICKS;
    }

    public virtual void onDeath() {
        player.AddScore(score);
        float chance = experiencePercent * experienceDrop / 10;
        if (Random.value < getPowerupChance(chance))
            map.InstantiatePowerup(transform.position);
        else if (Random.value < getHealthChance(chance))
            map.InstantiateHealth(transform.position);
        else if (Random.value < experiencePercent)
            map.InstantiateExperience(experienceDrop, transform.position);
        GetComponent<Renderer>().enabled = false;
        StartCoroutine(destroy());
    }

    IEnumerator destroy()
    {
        yield return new WaitUntil(() => !audioSrc.isPlaying);
        Destroy(gameObject);
    }

    // Returns the damage this mob should deal after some time
    public float GetDamage() {
        return baseDamage * Mathf.Min(Mathf.Pow(player.Wave, 1.3f) / 40 + 1, 4);
    }

    // Returns the health that this mob should spawn with after some time
    public float GetHealth() {
        return Mathf.Floor(startingHealth * (Mathf.Pow(player.Wave, 2) / 150 + 1));
    }

    public float GetSpeed() {
        return movementSpeed * (1.5f - .5f * Mathf.Pow(1.03f, -.5f * player.Wave));
    }

    private float getDropChance() {
        return (Mathf.Log(player.Wave) + 1) / player.Wave;
    }

    private float getHealthChance(float chance) {
        return HEALTH_CHANCE * getDropChance();
    }

    private float getPowerupChance(float chance) {
        return POWERUP_CHANCE * getDropChance();
    }
    
    public virtual void PlaySound()
    {
        if (audioSrc == null || dead) return;
        audioSrc.Play(audioDelay);
    }
}
