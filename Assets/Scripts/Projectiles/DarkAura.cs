using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DarkAura : Projectile
{
    private static AudioSource _audioSrc;

    public override string GetUpgradeEffect(int level, ProjectileStats next) {
        string message = base.GetUpgradeEffect(level, next);
        float size = stats.speed;
        if (next.speed != size) {
            float dSize = next.speed - size;
            message += (dSize > 0 ? "<color=green>+" : "<color=red>-") + ProjectileStats.Round(Mathf.Abs(100f * dSize / size), 1) + "% Size</color>\n";
        }
        return message;
    }

    public override string GetBaseStats(ProjectileStats next)
    {
        string message = base.GetBaseStats(next);
        message += "<color=green>Size: " + next.speed + "</color>\n";
        return message;
    }

    public override void GenerateStats(Transform playerTransform, int index = 1)
    {
        transform.localScale = new Vector3(stats.speed, stats.speed, 1);
        transform.eulerAngles = new Vector3(0, 0, Player.Updates * 1f);
    }

    public override void PlaySound()
    {
        if (!_audioSrc) _audioSrc = GameObject.Find("Dark Aura").GetComponent<AudioSource>();
        if(!_audioSrc.isPlaying && !GameTime.isPaused) _audioSrc.Play();
    }
}
