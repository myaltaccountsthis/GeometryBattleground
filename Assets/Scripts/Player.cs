using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public static int Updates {
        get => updates;
    }
    private static int updates;

    public int Wave {
        get => wave;
    }

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
    public GameObject upgradeUI;
    public Explosion explosionPrefab;
    public Spike spikePrefab;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI waveText;

    [SerializeField]
    private float health;
    private int level;
    private int experience;
    private int score;
    private Dictionary<string, int> ownedProjectiles;
    private int iFrames;
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, ProjectileStats[]> projectileInfo;
    private UpgradeOption[] upgradeUIOptions;
    private int wave;
    private int waveTextStart;
    private Map map;
    private CircleCollider2D playerCollider;

    [SerializeField, Tooltip("Basically Hacks")]
    private bool testingMode;

    private const int MAX_PROJECTILE_LEVEL = 8;

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
        map = GameObject.FindWithTag("Map").GetComponent<Map>();
        playerCollider = GetComponent<CircleCollider2D>();
        ownedProjectiles = new Dictionary<string, int>();
        ownedProjectiles.Add("Ball", 1);
        // TESTING
        // TESTING END
        health = totalHealth;
        iFrames = 0;
        updates = 0;
        level = 1;
        experience = 0;
        score = 0;
        wave = 0;
        waveTextStart = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateExpBar();
        UpdateScore();
        try {
            projectileInfo = new Dictionary<string, ProjectileStats[]>();
            string[] lines = infoFile.text.Split('\n');
            string[] headers = lines[0].Split(',');
            for (int i = 1; i < lines.Length; i++) {
                string[] lineInfo = lines[i].Split(',');
                int level = int.Parse(lineInfo[1]);
                string name = lineInfo[0];
                if (!projectileInfo.ContainsKey(name)) {
                    projectileInfo[name] = new ProjectileStats[MAX_PROJECTILE_LEVEL + 1];
                    projectileInfo[name][0] = ProjectileStats.Empty;
                }
                projectileInfo[name][level] = ProjectileStats.Parse(lineInfo, 2);
            }
        }
        catch (System.Exception e) {
            Debug.Log("Failed to read info file: " + e);
        }
        {
            Transform upgradeUIList = upgradeUI.transform.Find("Frame");
            upgradeUIOptions = new UpgradeOption[upgradeUIList.childCount];
            int i = 0;
            foreach (Transform t in upgradeUIList) {
                upgradeUIOptions[i] = t.GetComponent<UpgradeOption>();
                i++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameTime.isPaused)
            return;
        
        // check health
        if (health <= 0) {
            // TODO game over
            Application.Quit();
        }

        if (map.CurrentMobCount == 0 && !waveText.gameObject.activeSelf) {
            AdvanceWave();
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
        moveVector *= (testingMode ? movementSpeed * 2 : movementSpeed) / 60;
        position += moveVector;
        position.x = Mathf.Clamp(position.x, map.Bounds.xMin + playerCollider.radius, map.Bounds.xMax - playerCollider.radius);
        position.y = Mathf.Clamp(position.y, map.Bounds.yMin + playerCollider.radius, map.Bounds.yMax - playerCollider.radius);
        transform.position = position;
        Camera.main.transform.position = new Vector3(position.x, position.y, Camera.main.transform.position.z);
        Vector2 mousePosition = Input.mousePosition;
        float angle = Mathf.Atan2(mousePosition.y - Screen.height / 2, mousePosition.x - Screen.width / 2);
        transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * angle - 90);

        // projectiles
        foreach (string projectileName in ownedProjectiles.Keys) {
            ProjectileStats stats = GetProjectileStats(projectileName);
            Projectile projectile = projectileList[projectileName];
            if (updates % (testingMode ? Mathf.Max(stats.interval / 2, 1) : stats.interval) == 0) {
                projectile.toShoot = stats.projectileCount;
                projectile.toWait = 0;
            }
            while (projectile.toShoot > 0 && projectile.toWait == 0) {
                Projectile newProjectile = Instantiate<Projectile>(projectile, transform.position, Quaternion.identity, projectileFolder);
                newProjectile.LoadStats(stats);
                newProjectile.GenerateStats(transform, stats.projectileCount - projectile.toShoot);
                projectile.toShoot--;
                projectile.toWait = stats.timeBetweenShots;
            }
            projectile.toWait--;
        }

        // ui
        int waveTextAliveTime = Updates - waveTextStart;
        if (waveText.gameObject.activeSelf && waveTextAliveTime > 90) {
            waveText.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), (float) (waveTextAliveTime - 90) / 30);
            if (waveTextAliveTime > 120) {
                waveText.gameObject.SetActive(false);
                map.SpawnWave(wave);
            }
        }
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.white, .2f);
        healthBar.localScale = new Vector3(health / totalHealth, 1, 1);
        // UpdateExpBar();
    }

    void LateUpdate() {
        if (GameTime.isPaused)
            return;
        
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
            experience += testingMode ? exp.Value * 10 : exp.Value;
            score += exp.Value;
            CheckLevel();
            exp.PickUp(transform);
        }
    }

    private void AdvanceWave() {
        wave++;
        waveText.text = "Wave " + wave;
        waveText.color = Color.white;
        waveText.gameObject.SetActive(true);
        waveTextStart = Updates;
    }

    private void UpdateExpBar() {
        expBar.localScale = new Vector3(Mathf.Min(1f, (float)experience / ExpToNextLevel()), 1, 1);
        levelText.text = level + "";
    }

    public void CheckLevel() {
        int requiredExp = ExpToNextLevel();
        if (experience >= requiredExp) {
            int totalLevels = 0;
            foreach (string projectileName in ownedProjectiles.Keys)
                totalLevels += ownedProjectiles[projectileName];
            if (totalLevels < MAX_PROJECTILE_LEVEL * projectileList.Values.Count) {
                experience -= requiredExp;
                level++;
                UpdateExpBar();
                UpdateScore();
                ShowLevelUpUI();
                iFrames = 30;
            }
        }
        UpdateExpBar();
        UpdateScore();
    }

    private void UpdateScore() {
        scoreText.text = score + "";
    }

    public void AddScore(int toAdd) {
        score += toAdd;
        UpdateScore();
    }

    private int ExpToNextLevel() {
        // TODO also make leveling up more powerful (maybe ramp mobs more), TODO mobs
        return Mathf.FloorToInt(Mathf.Pow(level - 1, 1.5f)) + 1 * level + 4;
    }

    private ProjectileStats GetProjectileStats(string projectileName) {
        return projectileInfo[projectileName][ownedProjectiles[projectileName]];
    }

    public int GetProjectileLevel(string projectileName) {
        return ownedProjectiles[projectileName];
    }

    // show level up ui and generate 
    private void ShowLevelUpUI() {
        if (upgradeUI.activeInHierarchy)
            return;

        GameTime.isPaused = true;

        // generate options
        List<Projectile> availableProjectiles = new List<Projectile>();
        foreach (Projectile projectile in projectileList.Values) {
            if (!ownedProjectiles.ContainsKey(projectile.name) || ownedProjectiles[projectile.name] < MAX_PROJECTILE_LEVEL) {
                availableProjectiles.Add(projectile);
            }
        }
        // do shuffling
        for (int i = 0; i < availableProjectiles.Count - 1; i++) {
            int j = Random.Range(i + 1, availableProjectiles.Count);
            Projectile temp = availableProjectiles[i];
            availableProjectiles[i] = availableProjectiles[j];
            availableProjectiles[j] = temp;
        }

        // show the ui
        for (int i = 0; i < Mathf.Min(availableProjectiles.Count, 3); i++) {
            SetUpgradeUI(i, availableProjectiles[i]);
        }
        // if less than 2 upgrades available, make those upgrade options unavailable
        for (int i = 2; i >= availableProjectiles.Count; i--) {
            SetUpgradeUI(i, null);
        }
        upgradeUI.SetActive(true);
    }

    private void SetUpgradeUI(int index, Projectile projectile) {
        UpgradeOption option = upgradeUIOptions[index];
        if (projectile == null) {
            option.upgradeName.text = "NO UPGRADE";
            option.upgradeEffect.text = "";
            option.upgradeImage.sprite = null;
        }
        else {
            option.upgradeName.text = projectile.name;
            // TODO projectile upgrade effect description
            if (ownedProjectiles.ContainsKey(projectile.name)) {
                int level = ownedProjectiles[projectile.name];
                string message = projectileInfo[projectile.name][level].getUpgradeEffect(projectileInfo[projectile.name][level + 1]);
                option.upgradeEffect.text = message.Trim();
                option.upgradeLevel.text = "Level " + (level + 1);
            }
            else {
                option.upgradeEffect.text = "New projectile";
                option.upgradeLevel.text = "Unlock";
            }
            // TODO projectile image sprite
        }
    }

    public void OnUIOptionClick(UpgradeOption option) {
        if (!upgradeUI.activeInHierarchy)
            return;

        string upgradeName = option.upgradeName.text;
        if (!projectileList.ContainsKey(upgradeName))
            return;
        
        int i;
        for (i = 0; i < upgradeUIOptions.Length; i++) {
            if (upgradeUIOptions[i] == option)
                break;
        }

        if (!ownedProjectiles.ContainsKey(upgradeName))
            ownedProjectiles.Add(upgradeName, 0);
        ownedProjectiles[upgradeName]++;
        upgradeUI.SetActive(false);
        GameTime.isPaused = false;
        CheckLevel();
    }
}
