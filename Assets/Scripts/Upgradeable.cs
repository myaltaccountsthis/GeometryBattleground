using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgradeable
{
    public bool isProjectile {
        get => projectile != null;
    }
    public bool isPassive {
        get => passive != null;
    }
    public Projectile projectile;
    public Passive passive;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
