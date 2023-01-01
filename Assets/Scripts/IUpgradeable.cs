using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUpgradeable
{
    int GetLevel(Player player);
    string GetLevelText(int nextLevel);
    string GetName();
    string GetUpgradeEffect(Player player);
}
