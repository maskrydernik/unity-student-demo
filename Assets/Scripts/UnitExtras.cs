// UnitExtras.cs
// HP, world-space HP slider, heal-charge, gear tiers, gear HUD, speed-on-kill.
// Arthur(HP bar), Lionel(heal-charge full heal), Steven(gear tiers), Mary/VictorG(HUD),
// Jordon(speed on kill), Ryan/Christopher(hook OnEnemyKilled).

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class UnitExtras : MonoBehaviour
{
    [Header("Stats")]
    public int level = 1;
    public float maxHP = 100f;
    public float hp = 100f;            // read by attacker for kill-check
    public float baseDamage = 10f;
    public float moveSpeed = 3.5f;     // modified on kill
    public float speedGainOnKill = 0.15f;

    [Header("Heal Power (Lionel)")]
    public float healCharge = 0f;      // 0..1
    public float healGainPerPickup = 0.25f;

    [Header("Gear Tiers (Steven)")]
    public int weaponTier = 0;         // 0..5
    public int armorTier = 0;

    [Header("World UI (Arthur + Kameron)")]
    public Canvas worldCanvasPrefab;   // contains Image "HP" and Image "Rage"
    public float uiHeight = 2.0f;

    Image hpImage;
    Image rageImage;
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        Canvas c = Instantiate(worldCanvasPrefab, transform);
        c.transform.localPosition = new Vector3(0, uiHeight, 0);
        foreach (Image img in c.GetComponentsInChildren<Image>())
        {
            if (img.name == "HP")   hpImage   = img;
            if (img.name == "Rage") rageImage = img;
        }
        hpImage.fillAmount = hp / maxHP;
        rageImage.fillAmount = 0;

        UpdateGearHUD();
    }

    public void GainHealCharge()
    {
        healCharge = Mathf.Clamp01(healCharge + healGainPerPickup);
        UpdateGearHUD();
    }

    // Lionel: full-heal only when low and fully charged
    public void TryFullHeal()
    {
        if (hp <= maxHP * 0.5f && healCharge >= 1f)
        {
            hp = maxHP;
            healCharge = 0f;
            hpImage.fillAmount = hp / maxHP;
            UpdateGearHUD();
        }
    }

    public void TakeDamage(float dmg)
    {
        hp = Mathf.Max(0, hp - dmg);
        hpImage.fillAmount = hp / maxHP;
        if (hp <= 0) Die();
    }

    void Die()
    {
        AutoCombat ac = GetComponent<AutoCombat>();
        if (ac != null && ac.team == AutoCombat.Team.Enemy)
        {
            GameSystems.I.OnEnemyKilled(gameObject);
        }
        Destroy(gameObject);
    }

    // Steven: tier multipliers
    public float GetDamage()      { return baseDamage * (1f + 0.25f * weaponTier); }
    public float GetArmorFactor() { return 1f - 0.10f * armorTier; }

    // Mary/VictorG: equip + HUD
    public void EquipWeapon(int tier) { weaponTier = Mathf.Clamp(tier, 0, 5); UpdateGearHUD(); }
    public void EquipArmor (int tier) { armorTier  = Mathf.Clamp(tier, 0, 5); UpdateGearHUD(); }

    // Jordon: speed on kill (called by attacker)
    public void GainSpeedOnKill()
    {
        moveSpeed += speedGainOnKill;
        agent.speed = moveSpeed;
    }

    // Kameron: rage UI is updated by AutoCombat
    public void SetRageUI(float normalized) { rageImage.fillAmount = normalized; }

    void UpdateGearHUD()
    {
        GameSystems.I.UpdateGearHUD(
            "Gear: W+" + weaponTier + " A+" + armorTier + " | Heal:" + Mathf.RoundToInt(healCharge * 100) + "%"
        );
    }
}
