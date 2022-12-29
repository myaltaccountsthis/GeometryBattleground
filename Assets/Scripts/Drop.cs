using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Drop : MonoBehaviour
{
    [HideInInspector]
    public bool CanPickUp = true;
    [HideInInspector]
    public bool LargeDropSize {
        get => largeDropSize;
        set {
            if (value != LargeDropSize) {
                largeDropSize = value;
                myCollider.radius = largeDropSize ? originalRadius * 2 : originalRadius;
            } 
        }
    }

    protected Transform toFollow;
    protected Vector3 followOrigin;
    protected int followTicks;
    private Vector3 pushTo;
    private bool shouldPush;
    private bool largeDropSize;
    private float originalRadius;
    private CircleCollider2D myCollider;

    protected const int FOLLOW_TICKS = 10;
    protected const float LIFETIME = 60f;
    protected const float PUSH_SPEED = .1f;

    void Awake() {
        myCollider = GetComponent<CircleCollider2D>();
        Debug.Assert(myCollider != null, "Error: this drop is missing a CircleCollider2D");
        Debug.Assert(myCollider.isTrigger, "Error: this drop's Collider2D property isTrigger is false");
        BoundsInt bounds = GameObject.FindWithTag("Map").GetComponent<Map>().Bounds;
        Vector3 position = transform.position;
        float radius = myCollider.radius;
        pushTo = new Vector3(Mathf.Clamp(position.x, bounds.xMin + radius, bounds.xMax - radius), Mathf.Clamp(position.y, bounds.yMin + radius, bounds.yMax - radius), position.z);
        shouldPush = pushTo != position;
        originalRadius = radius;
    }

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, LIFETIME);
    }

    public virtual void PickUp(Player player) {
        toFollow = player.transform;
        followOrigin = transform.position;
        followTicks = FOLLOW_TICKS;
        CanPickUp = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldPush) {
            Vector3 direction = (pushTo - transform.position);
            float distance = direction.magnitude;
            if (distance < PUSH_SPEED) {
                transform.position = pushTo;
                shouldPush = false;
            }
            else
                transform.position += direction.normalized * PUSH_SPEED;
        }
        if (CanPickUp)
            return;
        if (followTicks == 0) {
            Destroy(gameObject);
            return;
        }
        followTicks--;
        transform.position = Vector3.Lerp(followOrigin, toFollow.position, (float) (FOLLOW_TICKS - followTicks) / FOLLOW_TICKS);
    }
}
