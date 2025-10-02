// Mary_HUD.cs
// Owns HUD text fields and exposes simple setters used by others.
// Now includes VictorG's gear display functionality.
using UnityEngine;
using TMPro;

public class Mary_HUD : MonoBehaviour
{
    public TMP_Text goldText;
    public TMP_Text gearText;
    public TMP_Text questText;
    public TMP_Text tipText;

    void Start()
    {
        if (GameGlue.I == null)
        {
            return;
        }

        GameGlue.I.goldText = goldText;
        GameGlue.I.gearText = gearText;
        GameGlue.I.questText = questText;
        GameGlue.I.tipText = tipText;
        GameGlue.I.RefreshHUD();
    }

    // VictorG: Refresh gear HUD for a specific unit
    public static void RefreshGearHUD(GameObject unit)
    {
        if (GameGlue.I == null || GameGlue.I.gearText == null)
        {
            return;
        }

        var gear = unit.GetComponent<Steven_GearStats>();
        var heal = unit.GetComponent<Lionel_HealCharge>();

        int weaponTier = gear ? gear.weaponTier : 0;
        int armorTier = gear ? gear.armorTier : 0;
        int healPercent = heal ? Mathf.RoundToInt(heal.healCharge * 100f) : 0;

        GameGlue.I.gearText.text = "Gear: W+" + weaponTier + " A+" + armorTier + " | Heal:" + healPercent + "%";
    }
}
