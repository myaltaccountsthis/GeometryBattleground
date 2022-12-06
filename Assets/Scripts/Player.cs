using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static int Updates {
        get => updates;
    }
    private static int updates;

    [Tooltip("Movement speed of the player in units per second")]
    public float movementSpeed;
    [Tooltip("Total health for the player, in health points")]
    public float totalHealth;
    [Tooltip("Health regeneration for the player, in health points per second")]
    public float healthRegen;
    [Tooltip("List of all projectiles to use")]
    public Projectile[] projectiles;
    [HideInInspector]
    public Dictionary<string, Projectile> projectileList = new Dictionary<string, Projectile>();
    public RectTransform healthBar;
    public RectTransform expBar;
    public Transform projectileFolder;
    [Tooltip("CSV file that contains data for each projectile")]
    public TextAsset infoFile;

    [SerializeField]
    private float health;
    private int level;
    private int experience;
    private Dictionary<Projectile, int> ownedProjectiles;
    private int iFrames;
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, Dictionary<string, ProjectileStats>> projectileInfo;

    [SerializeField]
    private bool testingMode;

    void Awake()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
        foreach (Projectile projectile in projectiles) {
            projectileList.Add(projectile.name, projectile);
            Debug.Assert(projectile != null, "Error: Projectile List contains null values");
        }
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void Start()
    {
        ownedProjectiles = new Dictionary<Projectile, int>();
        ownedProjectiles.Add(projectileList["Ball"], 1);
        // TESTING
        ownedProjectiles.Add(projectileList["Barrage"], 1);
        // TESTING END
        health = totalHealth;
        iFrames = 0;
        updates = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
        try {
            projectileInfo = new Dictionary<string, Dictionary<string, ProjectileStats>>();
            string[] lines = infoFile.text.Split('\n');
            string[] headers = lines[0].Split(',');
            for (int i = 1; i < lines.Length; i++) {
                string[] lineInfo = lines[i].Split(',');
                int level = int.Parse(lineInfo[1]);
            // TODO set up projectileInfo, create new constructor with stuff or something idk
            }
        }
        catch (System.Exception e) {
            Debug.Log("Failed to read info file: " + e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // check health
        if (health <= 0) {
            // TODO game over, uncomment taking damage in OnTriggerEnter2D
        }
        health = Mathf.Min(totalHealth, health + healthRegen / 60f);
        if (iFrames > 0)
            iFrames--;

        // do position stuff
        float horizontal = Input.GetAxis("Horizontal"), vertical = Input.GetAxis("Vertical");
        Vector2 position = transform.position;
        Vector2 moveVector = new Vector2(horizontal, vertical);
        if (moveVector.magnitude > 1)
            moveVector.Normalize();
        moveVector *= movementSpeed / 60;
        position += moveVector;
        transform.position = position;
        Camera.main.transform.position = new Vector3(position.x, position.y, Camera.main.transform.position.z);
        Vector2 mousePosition = Input.mousePosition;
        float angle = Mathf.Atan2(mousePosition.y - Screen.height / 2, mousePosition.x - Screen.width / 2);
        transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * angle - 90);

        // projectiles
        foreach (Projectile projectile in ownedProjectiles.Keys) {
            if (updates % projectile.stats.interval == 0 || testingMode) {
                projectile.toShoot = projectile.stats.projectileCount;
                projectile.toWait = 0;
            }
            while (projectile.toShoot > 0 && projectile.toWait == 0) {
                Projectile newProjectile = Instantiate<Projectile>(projectile, transform.position, Quaternion.identity, projectileFolder);
                newProjectile.GenerateStats(transform, projectile.stats.projectileCount - projectile.toShoot);
                projectile.toShoot--;
                projectile.toWait = projectile.stats.timeBetweenShots;
            }
            projectile.toWait--;
        }

        spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.white, .2f);
        healthBar.localScale = new Vector3(health / totalHealth, 1, 1);
        expBar.localScale = new Vector3((float)experience / ExpToNextLevel(), 1, 1);
    }

    void LateUpdate() {
        updates++;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Mob mob = collider.GetComponent<Mob>();
        if (mob != null) {
            if (iFrames > 0)
                return;
            
            health -= mob.GetDamage();
            spriteRenderer.color = Color.red;
            iFrames = 30;
        }
        Experience exp = collider.GetComponent<Experience>();
        if (exp != null && exp.CanPickUp) {
            experience += exp.Value;
            CheckLevel();
            exp.PickUp(transform);
        }
    }

    public void CheckLevel() {
        int requiredExp = ExpToNextLevel();
        if (experience >= requiredExp) {
            experience -= requiredExp;
            level++;
            // TODO do level up
            
        }
    }

    private int ExpToNextLevel() {
        return level * (level + 2) + 5;
    }
}
