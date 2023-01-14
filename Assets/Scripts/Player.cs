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
        get => dataManager.wave;
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
    public Transform projectileFolder;
    [Tooltip("CSV file that contains data for each projectile")]
    public TextAsset infoFile;
    public GameObject upgradeUI;
    public Explosion explosionPrefab;
    public TextMeshProUGUI waveText;
    public HudUI hudUI;

    private float shield;
    private float originalMovementSpeed;
    private Dictionary<string, int> activePowerups;
    private Dictionary<string, ProjectileLauncher> projectileLaunchers;
    public Dictionary<string, Passive> passiveList;
    private int iFrames;
    private int doNotShoot;
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, ProjectileStats[]> projectileInfo;
    private UpgradeOption[] upgradeUIOptions;
    private int waveTextStart;
    private Map map;
    private CircleCollider2D playerCollider;
    private DataManager dataManager;
    // if waveText is big
    private bool waveTextActive;

    [SerializeField, Tooltip(".5x atk int, 5w^2 xp")]
    private bool testingMode;

    public const int MAX_PROJECTILE_LEVEL = 10;
    private const int NO_SHOOT_TIME = 60;

    void Awake()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        map = GameObject.FindWithTag("Map").GetComponent<Map>();
        playerCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dataManager = GameObject.FindWithTag("DataManager").GetComponent<DataManager>();
        projectileLaunchers = new Dictionary<string, ProjectileLauncher>();
        foreach (Projectile projectile in projectiles) {
            Debug.Assert(projectile != null, "Error: Projectile List contains null values");
            projectileLaunchers.Add(projectile.name, new ProjectileLauncher(projectile, this));
        }
        passiveList = new Dictionary<string, Passive>();
        foreach (Passive passive in passives) {
            Debug.Assert(passive != null, "Error: Passive List contains null values");
            passiveList.Add(passive.name, passive);
        }
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
        activePowerups = new Dictionary<string, int>();
        // TESTING
        // TESTING END
        iFrames = 0;
        doNotShoot = NO_SHOOT_TIME;
        updates = 0;
        waveText.text = "";
        waveTextStart = 0;
        waveTextActive = false;
        movementSpeed = GetMovementSpeed();

        UpdateExpBar();
        UpdateScore();
        hudUI.UpdateHealth(dataManager.health, totalHealth, shield);
        
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
        if (dataManager.health <= 0) {
            GetComponent<GameOverMenuHandler>().OpenUI();
            dataManager.health = 0;
        }

        if (map.CurrentMobCount == 0 && !waveTextActive) {
            AdvanceWave();
        }

        dataManager.health = Mathf.Min(totalHealth, dataManager.health + healthRegen / 60f);
        if (iFrames > 0)
            iFrames--;
        if (doNotShoot > 0)
            doNotShoot--;

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
        foreach (string projectileName in dataManager.ownedProjectiles.Keys) {
            ProjectileStats stats = GetProjectileAppliedStats(projectileName);
            ProjectileLauncher launcher = projectileLaunchers[projectileName];
            launcher.Update(stats);
            if (updates % (testingMode ? Mathf.Max(stats.interval / 2, 1) : stats.interval) == 0) {
                launcher.Shoot();
            }
            while (launcher.ShouldShoot) {
                if (doNotShoot == 0) {
                    Projectile newProjectile = Instantiate<Projectile>(launcher.Projectile, transform.position, Quaternion.identity, projectileFolder);
                    newProjectile.LoadStats(stats);
                    newProjectile.GenerateStats(transform, stats.projectileCount - launcher.ToShoot);
                }
                launcher.ShootSub();
            }
        }

        // ui
        int waveTextAliveTime = Updates - waveTextStart;
        if (waveTextActive) {
            if (waveTextAliveTime == 0)
                map.SpawnWave(Wave);
            if (waveTextAliveTime > 90) {
                float t = (float) (waveTextAliveTime - 90) / 30;
                // waveText.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), (float) (waveTextAliveTime - 90) / 30);
                waveText.fontSize = 80 - 36 * t;
                if (waveTextAliveTime > 120) {
                    waveTextActive = false;
                }
            }
        }
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.white, .2f);
        hudUI.UpdateHealth(dataManager.health, totalHealth, shield);
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
            TakeDamage(mob.GetDamage());
        }
        Drop drop = collider.GetComponent<Drop>();
        if (drop != null && drop.CanPickUp)
            drop.PickUp(this);
        EnemyExplosion enemyExplosion = collider.GetComponent<EnemyExplosion>();
        if (enemyExplosion != null) {
            TakeDamage(enemyExplosion.stats.damage);
        }
    }

    private void TakeDamage(float damage) {
        if (iFrames > 0)
            return;
        
        if (shield > 0) {
            float shieldDamage = Mathf.Min(damage, shield);
            shield -= shieldDamage;
            damage -= shieldDamage;
        }
        dataManager.health -= damage;
        spriteRenderer.color = Color.red;
        iFrames = 30;
    }

    private void AdvanceWave() {
        dataManager.wave++;
        dataManager.SaveData();
        waveText.text = "Wave " + Wave;
        // waveText.color = Color.white;
        waveText.fontSize = 80;
        waveTextActive = true;
        waveTextStart = Updates;    
    }

    // ui and projectile stats
    
    public void CollectExp(Experience exp) {
        dataManager.experience += Mathf.FloorToInt(exp.Value * GetExpMultiplier());
        AddScore(exp.Value);
        CheckLevel();
    }

    public void CollectHealth(Health healthDrop) {
        dataManager.health = totalHealth;
        AddScore(Health.SCORE);
    }

    private void UpdateExpBar() {
        hudUI.UpdateExpBar(dataManager.experience, ExpToNextLevel(), dataManager.level);
    }

    public void CheckLevel() {
        int requiredExp = ExpToNextLevel();
        if (dataManager.experience >= requiredExp) {
            // int totalLevels = 0;
            // foreach (string projectileName in dataManager.ownedProjectiles.Keys)
            //     totalLevels += dataManager.ownedProjectiles[projectileName];
            // if (totalLevels < MAX_PROJECTILE_LEVEL * projectileList.Values.Count) {
            dataManager.experience -= requiredExp;
            dataManager.level++;
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
        if (dataManager.score > dataManager.highScore)
            dataManager.highScore = dataManager.score;
        hudUI.UpdateScore(dataManager.score);
    }

    public void AddScore(int toAdd) {
        dataManager.score += toAdd;
        UpdateScore();
    }

    private int ExpToNextLevel() {
        // TODO also make leveling up more powerful (maybe ramp mobs more), TODO mobs
        return Mathf.FloorToInt((Mathf.FloorToInt(Mathf.Pow(dataManager.level - 1, 1.5f)) + 1 * dataManager.level + 4) * 10);
    }

    public ProjectileStats GetProjectileStats(string projectileName, int projLevel = -1) {
        if (projLevel == -1)
            projLevel = dataManager.ownedProjectiles[projectileName];
        return projectileInfo[projectileName][projLevel];
    }

    public ProjectileStats GetProjectileAppliedStats(string projectileName) {
        ProjectileStats stats = GetProjectileStats(projectileName);
        stats.ApplyPassives(dataManager.ownedPassives);
        if (activePowerups.ContainsKey("Infinite Pierce"))
            stats.pierce = -1;
        return stats;
    }

    public int GetProjectileLevel(string projectileName) {
        int projLevel = 0;
        dataManager.ownedProjectiles.TryGetValue(projectileName, out projLevel);
        return projLevel;
    }

    public int GetPassiveLevel(string passiveName) {
        int passiveLevel = 0;
        dataManager.ownedPassives.TryGetValue(passiveName, out passiveLevel);
        return passiveLevel;
    }

    // show dataManager.level up ui and generate 
    private void ShowLevelUpUI() {
        if (upgradeUI.activeInHierarchy)
            return;

        GameTime.isPaused = true;

        // generate options
        List<IUpgradeable> availableUpgrades = new List<IUpgradeable>();
        foreach (ProjectileLauncher launcher in projectileLaunchers.Values) {
            Projectile projectile = launcher.Projectile;
            if (!dataManager.ownedProjectiles.ContainsKey(projectile.name) || dataManager.ownedProjectiles[projectile.name] < MAX_PROJECTILE_LEVEL) {
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
            option.upgradeImage.sprite = upgrade.GetSprite();
            // TODO projectile image sprite
            
            /*
            if (upgrade.isProjectile) {
                Projectile projectile = upgrade.projectile;
                option.upgradeName.text = projectile.name;
                if (dataManager.ownedProjectiles.ContainsKey(projectile.name)) {
                    int upgradeLevel = dataManager.ownedProjectiles[projectile.name];
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
                int upgradeLevel = dataManager.ownedPassives[passive.name];
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
        if (!projectileLaunchers.ContainsKey(upgradeName) && !passiveList.ContainsKey(upgradeName))
            return;
        
        int i;
        for (i = 0; i < upgradeUIOptions.Length; i++) {
            if (upgradeUIOptions[i] == option)
                break;
        }

        if (projectileLaunchers.ContainsKey(upgradeName)) {
            if (!dataManager.ownedProjectiles.ContainsKey(upgradeName))
                dataManager.ownedProjectiles.Add(upgradeName, 0);
            dataManager.ownedProjectiles[upgradeName]++;
        }
        else {
            if (!dataManager.ownedPassives.ContainsKey(upgradeName))
                dataManager.ownedPassives.Add(upgradeName, 0);
            dataManager.ownedPassives[upgradeName]++;
        }
        upgradeUI.SetActive(false);
        GameTime.isPaused = false;
        printInfo();
        CheckLevel();
    }

    private void printInfo() {
        string message = "L: " + dataManager.level + ", ";
        foreach (string name in dataManager.ownedProjectiles.Keys)
            message += name + ": " + dataManager.ownedProjectiles[name] + ", ";
        message += "\n";
        foreach (string name in dataManager.ownedPassives.Keys)
            message += name + ": " + dataManager.ownedPassives[name] + ", ";
        Debug.Log(message);
    }

    // powerups

    public void CollectPowerup(Powerup powerup) {
        activePowerups[powerup.type] = powerup.duration;
        AddScore(Powerup.SCORE);
        switch (powerup.type) {
            case "Nuke":
                Explosion explosion = Instantiate<Explosion>(explosionPrefab, powerup.transform.position, Quaternion.identity, projectileFolder);
                explosion.LoadStats(new ProjectileStats(0, 0, 0, 100, 0, -1, 0, 0, 0));
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
        // something strange here where during map's Start(), activePowerups is null
        return activePowerups != null && activePowerups.ContainsKey(powerupName);
    }

    // passives
    public float GetMovementSpeed() {
        return (originalMovementSpeed + dataManager.ownedPassives.GetValueOrDefault("Agility", 0)) * (activePowerups.ContainsKey("Speed") ? 1.5f : 1);
    }

    public float GetExpMultiplier() {
        return (1 + .2f * dataManager.ownedPassives.GetValueOrDefault("Wisdom", 0)) * (testingMode ? 5 * Mathf.Pow(Wave, 2) : 1);
    }
}
