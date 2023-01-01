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
    [Tooltip("List of all passive abilities")]
    public Passive[] passives;
    [HideInInspector]
    public Dictionary<string, Projectile> projectileList;
    [HideInInspector]
    public Dictionary<string, Passive> passiveList;
    public Transform projectileFolder;
    [Tooltip("CSV file that contains data for each projectile")]
    public TextAsset infoFile;
    public GameObject upgradeUI;
    public Explosion explosionPrefab;
    public Spike spikePrefab;
    public TextMeshProUGUI waveText;
    public HudUI hudUI;

    [SerializeField]
    private float health;
    private float shield;
    private int level;
    private int experience;
    private int score;
    private float originalMovementSpeed;
    private Dictionary<string, int> ownedProjectiles;
    private Dictionary<string, int> ownedPassives;
    private Dictionary<string, int> activePowerups;
    private int iFrames;
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, ProjectileStats[]> projectileInfo;
    private UpgradeOption[] upgradeUIOptions;
    private int wave;
    private int waveTextStart;
    private Map map;
    private CircleCollider2D playerCollider;

    [SerializeField, Tooltip(".5x atk int, 10x xp")]
    private bool testingMode;

    private const int MAX_PROJECTILE_LEVEL = 8;

    void Awake()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        projectileList = new Dictionary<string, Projectile>();
        foreach (Projectile projectile in projectiles) {
            projectileList.Add(projectile.name, projectile);
            Debug.Assert(projectile != null, "Error: Projectile List contains null values");
        }
        passiveList = new Dictionary<string, Passive>();
        ownedPassives = new Dictionary<string, int>();
        foreach (Passive passive in passives) {
            passiveList.Add(passive.name, passive);
            ownedPassives.Add(passive.name, 0);
            Debug.Assert(passive != null, "Error: Passive List contains null values");
        }
        activePowerups = new Dictionary<string, int>();
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
        originalMovementSpeed = movementSpeed;
        try {
            projectileInfo = new Dictionary<string, ProjectileStats[]>();
            string[] lines = infoFile.text.Split('\n');
            string[] headers = lines[0].Split(',');
            for (int i = 1; i < lines.Length; i++) {
                string[] lineInfo = lines[i].Split(',');
                int projLevel = int.Parse(lineInfo[1]);
                string name = lineInfo[0];
                if (!projectileInfo.ContainsKey(name)) {
                    projectileInfo[name] = new ProjectileStats[MAX_PROJECTILE_LEVEL + 1];
                    projectileInfo[name][0] = ProjectileStats.Empty;
                }
                projectileInfo[name][projLevel] = ProjectileStats.Parse(lineInfo, 2);
            }
        }
        catch (System.Exception e) {
            Debug.Log("Failed to read info file: " + e);
        }
    }

    void Start()
    {
        map = GameObject.FindWithTag("Map").GetComponent<Map>();
        playerCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        UpdateExpBar();
        UpdateScore();
        hudUI.UpdateHealth(health, totalHealth, shield);
        
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
            stats.ApplyPassives(ownedPassives);
            if (activePowerups.ContainsKey("Infinite Pierce"))
                stats.pierce = -1;
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
        hudUI.UpdateHealth(health, totalHealth, shield);
        if (shield > 0) {
            // TODO shield visual
        }
        hudUI.UpdatePowerups(activePowerups);
        // UpdateExpBar();

        // powerups
        List<string> powerupNames = new List<string>(activePowerups.Keys);
        foreach (string powerupName in powerupNames) {
            activePowerups[powerupName]--;
            if (activePowerups[powerupName] <= 0) {
                activePowerups.Remove(powerupName);
                switch (powerupName) {
                    case "Drop Magnet":
                        foreach (Transform transform in map.experienceFolder) {
                            transform.GetComponent<Drop>().LargeDropSize = true;
                        }
                        break;
                    case "Speed":
                        movementSpeed = GetMovementSpeed();
                        break;
                    case "Shield":
                        shield = 0;
                        break;
                }
            }
        }
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
            
            TakeDamage(mob.GetDamage());
            spriteRenderer.color = Color.red;
            iFrames = 30;
        }
        Drop drop = collider.GetComponent<Drop>();
        if (drop != null && drop.CanPickUp)
            drop.PickUp(this);
    }

    private void TakeDamage(float damage) {
        if (shield > 0) {
            float shieldDamage = Mathf.Min(damage, shield);
            shield -= shieldDamage;
            damage -= shieldDamage;
        }
        health -= damage;
    }

    private void AdvanceWave() {
        wave++;
        waveText.text = "Wave " + wave;
        waveText.color = Color.white;
        waveText.gameObject.SetActive(true);
        waveTextStart = Updates;    
    }

    // ui and projectile stats
    
    public void CollectExp(Experience exp) {
        experience += Mathf.FloorToInt(exp.Value * GetExpMultiplier());
        score += exp.Value;
        CheckLevel();
    }

    public void CollectHealth(Health healthDrop) {
        health = totalHealth;
    }

    private void UpdateExpBar() {
        hudUI.UpdateExpBar(experience, ExpToNextLevel(), level);
    }

    public void CheckLevel() {
        int requiredExp = ExpToNextLevel();
        if (experience >= requiredExp) {
            // int totalLevels = 0;
            // foreach (string projectileName in ownedProjectiles.Keys)
            //     totalLevels += ownedProjectiles[projectileName];
            // if (totalLevels < MAX_PROJECTILE_LEVEL * projectileList.Values.Count) {
            experience -= requiredExp;
            level++;
            UpdateExpBar();
            UpdateScore();
            ShowLevelUpUI();
            iFrames = 30;
        }
        UpdateExpBar();
        UpdateScore();
        movementSpeed = GetMovementSpeed();
    }

    private void UpdateScore() {
        hudUI.UpdateScore(score);
    }

    public void AddScore(int toAdd) {
        score += toAdd;
        UpdateScore();
    }

    private int ExpToNextLevel() {
        // TODO also make leveling up more powerful (maybe ramp mobs more), TODO mobs
        return Mathf.FloorToInt((Mathf.FloorToInt(Mathf.Pow(level - 1, 1.5f)) + 1 * level + 4) * 10);
    }

    public ProjectileStats GetProjectileStats(string projectileName, int projLevel = -1) {
        if (projLevel == -1)
            projLevel = ownedProjectiles[projectileName];
        return projectileInfo[projectileName][projLevel];
    }

    public int GetProjectileLevel(string projectileName) {
        int projLevel;
        ownedProjectiles.TryGetValue(projectileName, out projLevel);
        return projLevel;
    }

    public int GetPassiveLevel(string passiveName) {
        int passiveLevel;
        ownedPassives.TryGetValue(passiveName, out passiveLevel);
        return passiveLevel;
    }

    // show level up ui and generate 
    private void ShowLevelUpUI() {
        if (upgradeUI.activeInHierarchy)
            return;

        GameTime.isPaused = true;

        // generate options
        List<IUpgradeable> availableUpgrades = new List<IUpgradeable>();
        foreach (Projectile projectile in projectileList.Values) {
            if (!ownedProjectiles.ContainsKey(projectile.name) || ownedProjectiles[projectile.name] < MAX_PROJECTILE_LEVEL) {
                availableUpgrades.Add(projectile);
            }
        }
        foreach (Passive passive in passiveList.Values) {
            availableUpgrades.Add(passive);
        }
        // do shuffling
        for (int i = 0; i < availableUpgrades.Count - 1; i++) {
            int j = Random.Range(i + 1, availableUpgrades.Count);
            IUpgradeable temp = availableUpgrades[i];
            availableUpgrades[i] = availableUpgrades[j];
            availableUpgrades[j] = temp;
        }

        // show the ui
        for (int i = 0; i < Mathf.Min(availableUpgrades.Count, 3); i++) {
            SetUpgradeUI(i, availableUpgrades[i]);
        }
        // if less than 2 upgrades available, make those upgrade options unavailable
        for (int i = 2; i >= availableUpgrades.Count; i--) {
            SetUpgradeUI(i, null);
        }
        upgradeUI.SetActive(true);
    }

    private void SetUpgradeUI(int index, IUpgradeable upgrade) {
        UpgradeOption option = upgradeUIOptions[index];
        if (upgrade == null) {
            option.upgradeName.text = "NO UPGRADE";
            option.upgradeEffect.text = "";
            option.upgradeImage.sprite = null;
        }
        else {
            option.upgradeName.text = upgrade.GetName();
            int upgradeLevel = upgrade.GetLevel(this);
            option.upgradeLevel.text = upgrade.GetLevelText(upgradeLevel + 1);
            option.upgradeEffect.text = upgrade.GetUpgradeEffect(this);
            // TODO projectile image sprite
            
            /*
            if (upgrade.isProjectile) {
                Projectile projectile = upgrade.projectile;
                option.upgradeName.text = projectile.name;
                if (ownedProjectiles.ContainsKey(projectile.name)) {
                    int upgradeLevel = ownedProjectiles[projectile.name];
                    projectile.stats = projectileInfo[projectile.name][upgradeLevel];
                    string message = projectile.GetUpgradeEffect(upgradeLevel, projectileInfo[projectile.name][upgradeLevel + 1]);
                    option.upgradeEffect.text = message.Trim();
                    option.upgradeLevel.text = "Level " + (upgradeLevel + 1);
                }
                else {
                    option.upgradeEffect.text = "New projectile";
                    option.upgradeLevel.text = "Unlock";
                }
            }
            else if (upgrade.isPassive) {
                Passive passive = upgrade.passive;
                option.upgradeName.text = passive.name;
                int upgradeLevel = ownedPassives[passive.name];
                string message = passive.GetUpgradeEffect();
                option.upgradeEffect.text = message;
                option.upgradeLevel.text = "Level " + (upgradeLevel + 1);
            }
            */
        }
    }

    public void OnUIOptionClick(UpgradeOption option) {
        if (!upgradeUI.activeInHierarchy)
            return;

        string upgradeName = option.upgradeName.text;
        if (!projectileList.ContainsKey(upgradeName) && !passiveList.ContainsKey(upgradeName))
            return;
        
        int i;
        for (i = 0; i < upgradeUIOptions.Length; i++) {
            if (upgradeUIOptions[i] == option)
                break;
        }

        if (projectileList.ContainsKey(upgradeName)) {
            if (!ownedProjectiles.ContainsKey(upgradeName) && projectileList.ContainsKey(upgradeName))
                ownedProjectiles.Add(upgradeName, 0);
            ownedProjectiles[upgradeName]++;
        }
        else {
            ownedPassives[upgradeName]++;
        }
        upgradeUI.SetActive(false);
        GameTime.isPaused = false;
        printInfo();
        CheckLevel();
    }

    private void printInfo() {
        string message = "L: " + level + ", ";
        foreach (string name in ownedProjectiles.Keys)
            message += name + ": " + ownedProjectiles[name] + ", ";
        message += "\n";
        foreach (string name in ownedPassives.Keys)
            message += name + ": " + ownedPassives[name] + ", ";
        Debug.Log(message);
    }

    // powerups

    public void CollectPowerup(Powerup powerup) {
        activePowerups[powerup.type] = powerup.duration;
        switch (powerup.type) {
            case "Nuke":
                Explosion explosion = Instantiate<Explosion>(explosionPrefab, powerup.transform.position, Quaternion.identity, projectileFolder);
                explosion.LoadStats(new ProjectileStats(0, 0, 0, 1000000, 0, -1, 0, 0, 0));
                explosion.SetOriginalColor(new Color(.5f, .06f, .02f, .3f));
                explosion.minSize = 1f;
                explosion.maxSize = map.Bounds.size.magnitude / 2.56f;
                explosion.explosionLifeTime = 180;
                explosion.explosionDecayTime = 60;
                break;
            case "Drop Magnet":
                foreach (Transform transform in map.experienceFolder) {
                    transform.GetComponent<Drop>().LargeDropSize = true;
                }
                break;
            case "Speed":
                movementSpeed = GetMovementSpeed();
                break;
            case "Shield":
                shield = totalHealth;
                break;
        }
    }

    public bool IsPowerupActive(string powerupName) {
        return activePowerups.ContainsKey(powerupName);
    }

    // passives
    public float GetMovementSpeed() {
        return (originalMovementSpeed + ownedPassives["Agility"]) * (activePowerups.ContainsKey("Speed") ? 1.5f : 1);
    }

    public float GetExpMultiplier() {
        return (1 + .2f * ownedPassives["Wisdom"]) * (testingMode ? 10 : 1);
    }
}
