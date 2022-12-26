using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Drop : MonoBehaviour
{
    [HideInInspector]
    public bool CanPickUp = true;

    protected Transform toFollow;
    protected Vector3 followOrigin;
    protected int followTicks;

    protected const int FOLLOW_TICKS = 10;
    protected const float LIFETIME = 60f;

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
