using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : Projectile
{
    // THESE 4 VARIABLES MUST BE INITIALIZED WHEN INSTANTIATED
    public float minSize;
    public float maxSize;
    public int explosionLifeTime;
    public int explosionDecayTime;

    private Vector3 startPosition;
    private int aliveFor;
    private SpriteRenderer spriteRenderer;
    private Collider2D collision2;
    private Color originalColor;

    public override void Awake()
    {
        base.Awake();

        startPosition = transform.position;
        aliveFor = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
        collision2 = GetComponent<Collider2D>();
        originalColor = spriteRenderer.color;
        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    public override void Update()
    {
        aliveFor++;

        if (aliveFor >= explosionLifeTime) {
            collision2.enabled = false;
            spriteRenderer.color = Color.Lerp(originalColor, Color.clear, (float) (aliveFor - explosionLifeTime) / explosionDecayTime);
        }
        else {
            float scale = (maxSize - minSize) * aliveFor / explosionLifeTime + minSize;
            transform.localScale = new Vector3(scale, scale, 1);
        }
        transform.position = startPosition;

        if (aliveFor >= explosionLifeTime + explosionDecayTime)
            Destroy(gameObject);
    }

    public override void GenerateStats(Transform playerTransform, int index = 1)
    {
        // things that create an explosion will just give their stats to this
    }

    public void SetOriginalColor(Color color) {
        GetComponent<SpriteRenderer>().color = color;
    }
}
