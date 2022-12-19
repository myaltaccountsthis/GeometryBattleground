using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeOption : MonoBehaviour, IPointerDownHandler
{
    public TextMeshProUGUI upgradeName;
    public TextMeshProUGUI upgradeEffect;
    public TextMeshProUGUI upgradeLevel;
    public Image upgradeImage;

    private Player player;

    void Start() {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        player.OnUIOptionClick(this);
    }
}
