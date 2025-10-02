// Steven_GearStats.cs
// Holds tiers and computes damage/armor factors.
using UnityEngine;

public class Steven_GearStats : MonoBehaviour
{
    public float baseDamage = 10f;
    public int weaponTier = 0; // 0..5
    public int armorTier = 0;  // 0..5

    public void SetWeaponTier(int tier)
    {
        weaponTier = Mathf.Clamp(tier, 0, 5);
        Mary_HUD.RefreshGearHUD(gameObject);
    }

    public void SetArmorTier(int tier)
    {
        armorTier = Mathf.Clamp(tier, 0, 5);
        Mary_HUD.RefreshGearHUD(gameObject);
    }

    public float GetDamage()
    {
        return baseDamage * (1f + 0.25f * weaponTier);
    }

    public float GetArmorFactor()
    {
        return 1f - 0.10f * armorTier;
    }
}
