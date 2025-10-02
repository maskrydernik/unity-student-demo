// Lionel_HealCharge.cs
// Heal meter can store multiple charges. Press H to spend 100% and heal to max.
using UnityEngine;

public class Lionel_HealCharge : MonoBehaviour
{
    public float healCharge = 0f;
    public float gainPerPickup = 0.5f;

    Arthur_WorldHPBar hpBar;

    void Start()
    {
        hpBar = GetComponent<Arthur_WorldHPBar>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && hpBar != null && healCharge >= 1f)
        {
            hpBar.hp = hpBar.maxHP;
            healCharge = Mathf.Max(0f, healCharge - 1f);
            hpBar.Sync();
            Mary_HUD.RefreshGearHUD(gameObject);
        }
    }
}
