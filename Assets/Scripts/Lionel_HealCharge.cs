// Lionel_HealCharge.cs
// Heal meter 0..1. Press H with a full charge to heal to max.
using UnityEngine;

public class Lionel_HealCharge : MonoBehaviour
{
    public float healCharge = 0f;
    public float gainPerPickup = 0.25f;

    Arthur_WorldHPBar hpBar;

    void Start(){ hpBar = GetComponent<Arthur_WorldHPBar>(); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && hpBar && healCharge >= 1f)
        {
            hpBar.hp = hpBar.maxHP;
            healCharge = 0f;
            hpBar.Sync();
            Mary_HUD.RefreshGearHUD(gameObject);
        }
    }
}
