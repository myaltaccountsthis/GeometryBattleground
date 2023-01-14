using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

// there will be instances of each sub-class in Player's projectiles instance variable. those will Instantiate<>() new projectiles
public abstract class Projectile : MonoBehaviour, IUpgradeable
{
    public ProjectileStats stats;

    public float startAudioAt;
    // in radians
    [HideInInspector]
    public float angle;
    public Sprite sprite;
    
    [HideInInspector]
    public bool isSub;

    private Collider2D collision;
    protected int creationTime;
    private int pierceLeft;
    private BoundsInt bounds;
    protected AudioSource audioSrc;

    public virtual void Awake() {
        collision = GetComponent<Collider2D>();
        audioSrc = GetComponent<AudioSource>();
        if(audioSrc != null) audioSrc.time = startAudioAt;
        Debug.Assert(collision.isTrigger, "Error: this projectile's Collider2D property isTrigger is false");
    }

    public virtual void Start()
    {
        bounds = GameObject.FindWithTag("Map").GetComponent<Map>().Bounds;
        creationTime = Player.Updates;
    }

    public virtual void Update()
    {
        if (GameTime.isPaused)
            return;

        if (Player.Updates >= creationTime + stats.lifeTime || !bounds.Contains(Vector3Int.FloorToInt(transform.position))) {
            Destroy(gameObject);
        }
        
        float currentSpeed = Mathf.Max((stats.speed - stats.drag * (Player.Updates - creationTime) / 60f) / 60f, 0f);
        transform.position += new Vector3(Mathf.Cos(angle) * currentSpeed, Mathf.Sin(angle) * currentSpeed, 0);
    }

    // Get stats using projectilestats, should be used and overriden by children
    public virtual string GetUpgradeEffect(int level, ProjectileStats next) {
        string message = stats.GetBaseUpgradeEffect(next);
        return message;
    }
    public virtual string GetBaseStats(ProjectileStats next) {
        string message = next.GetBaseStats();
        return message;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Mob mob = collider.GetComponent<Mob>();
        if (mob != null) {
            if (pierceLeft == 0)
                return;
            pierceLeft--;
            mob.TakeDamage(this);
            if (pierceLeft == 0)
                Destroy(gameObject);
        }
    }

    // Generate random instance variable values for projectile
    public abstract void GenerateStats(Transform playerTransform, int index = 1);
    // Load stats from the ones read from a text file
    public void LoadStats(ProjectileStats stats) {
        this.stats = stats;
        this.pierceLeft = stats.pierce;
    }

    public int GetLevel(Player player)
    {
        return player.GetProjectileLevel(name);
    }

    public string GetLevelText(int nextLevel) {
        // if (nextLevel == 1) {
        //     return "";
        // }
        if (nextLevel == Player.MAX_PROJECTILE_LEVEL)
            return "Level MAX";
        return "Level " + nextLevel;
    }

    public string GetName()
    {
        return name;
    }

    public string GetUpgradeEffect(Player player)
    {
        int currentLevel = player.GetProjectileLevel(name);
        ProjectileStats next = player.GetProjectileStats(name, currentLevel + 1);
        // when unlock
        if (currentLevel == 0) {
            return GetBaseStats(next);
        }

        // compare this level to next
        LoadStats(player.GetProjectileStats(name, currentLevel));
        return GetUpgradeEffect(currentLevel, player.GetProjectileStats(name, currentLevel + 1));
    }

    public Sprite GetSprite() {
        return sprite;
    }

    public virtual void PlaySound()
    {
        if (audioSrc == null) return;
        audioSrc.Play();
    }
}
