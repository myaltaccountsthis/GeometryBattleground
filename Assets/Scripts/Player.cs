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

    [SerializeField]
    private float health;
    private List<Projectile> ownedProjectiles;
    private int iFrames;

    void Awake()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
        foreach (Projectile projectile in projectiles) {
            projectileList.Add(projectile.name, projectile);
        }
        updates = 0;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        ownedProjectiles = new List<Projectile>();
        ownedProjectiles.Add(projectileList["Ball"]);
        health = totalHealth;
        iFrames = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // check health
        if (health <= 0) {
            // TODO game over
        }
        health = Mathf.Min(totalHealth, health + healthRegen / 60f);

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
        foreach (Projectile projectile in ownedProjectiles) {
            if (updates % projectile.interval == 0) {
                int count = projectile.GetCount();
                for (int i = 0; i < count; i++) {
                    Projectile newProjectile = Instantiate<Projectile>(projectile, transform.position, Quaternion.identity, GameObject.FindWithTag("Projectile Folder").transform);
                    newProjectile.GenerateStats(angle);
                }
            }
        }
    }

    void LateUpdate() {
        updates++;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Mob mob = collider.GetComponent<Mob>();
        if (mob != null) {
            if (iFrames > 0)
                return;

            
            iFrames = 30;
        }
    }
}
