// GearPickup.cs
// VictorG: pickups equip gear (weapon/armor tiers) and increase stats.
// Mary: HUD reflects current gear. Lionel: also charges heal meter.
// David: grants gold to tie back to shop.

using UnityEngine;

public class GearPickup : MonoBehaviour
{
    public bool isWeapon = true;
    public int tier = 1;
    public int goldValue = 5;
    public float healChargeBonus = 0.25f;

    void OnTriggerEnter(Collider other)
    {
        UnitExtras ex = other.GetComponent<UnitExtras>();
        if (ex != null)
        {
            if (isWeapon) ex.EquipWeapon(tier); else ex.EquipArmor(tier);
            ex.healCharge = Mathf.Clamp01(ex.healCharge + healChargeBonus);
            GameSystems.I.AddGold(goldValue);
            Destroy(gameObject);
        }
    }
}
