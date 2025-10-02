// VictorG_GearPickup.cs (renamed from VictorG_GearHUD.cs)
// VictorG: Gear pickup items - equips tier on trigger, grants gold, and bumps heal meter.
// HUD functionality moved to Mary_HUD.
using UnityEngine;

public class VictorG_GearPickup : MonoBehaviour
{
    public bool isWeapon = true;
    public int tier = 1;
    public int goldValue = 5;
    public float healBonus = 0.25f;

    void OnTriggerEnter(Collider other)
    {
        var g = other.GetComponent<Steven_GearStats>();
        var l = other.GetComponent<Lionel_HealCharge>();

        if (g != null)
        {
            if (isWeapon) g.SetWeaponTier(tier); else g.SetArmorTier(tier);
            if (l != null) l.healCharge = Mathf.Clamp01(l.healCharge + healBonus);
            GameGlue.I.AddGold(goldValue);
            Mary_HUD.RefreshGearHUD(other.gameObject);
            Destroy(gameObject);
        }
    }
}
