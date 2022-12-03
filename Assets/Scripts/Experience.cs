using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experience : MonoBehaviour
{
    public int Value {
        get => experienceValue;
    }
    public bool CanPickUp = true;

    private int experienceValue;
    private Transform toFollow;
    private Vector3 followOrigin;
    private int followTicks;

    private const int FOLLOW_TICKS = 10;
    private const float LIFETIME = 60f;

    void Start() {
        Destroy(gameObject, LIFETIME);
    }

    public void SetExperience(int experience) {
        experienceValue = experience;
    }

    public void PickUp(Transform follow) {
        toFollow = follow;
        followOrigin = transform.position;
        followTicks = FOLLOW_TICKS;
        CanPickUp = false;
    }

    void Update() {
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
