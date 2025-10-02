// Mary_WeaponBuy.cs
// Press T to talk when near. Pay 50 gold to upgrade weapon.

using UnityEngine;

public class JMary_WeaponBuy : MonoBehaviour
{
    [TextArea] public string greeting = "I can upgrade your weapon for 50 gold.";
    [TextArea] public string option1 = "Pay 50 gold (Upgrade)";
    [TextArea] public string option2 = "Never mind.";
    public float talkRange = 2f;

    bool inside = false;
    bool dialog = false;
    Transform currentListener;

    void Update()
    {
        UpdateProximity();

        if (!inside)
            return;

        if (!dialog && Input.GetKeyDown(KeyCode.T))
        {
            dialog = true;
            GameGlue.I?.Hint(greeting + " [1] " + option1 + "  [2] " + option2);
        }
        else if (dialog && Input.GetKeyDown(KeyCode.Alpha1))
        {
            dialog = false;
            var stats = currentListener.GetComponent<Steven_GearStats>();

            if (GameGlue.I.gold >= 50 && stats != null)
            {
                GameGlue.I.gold -= 50;
                stats.SetWeaponTier(stats.weaponTier + 1);
                GameGlue.I.Hint("Weapon upgraded! Tier: " + stats.weaponTier);
                GameGlue.I.RefreshHUD();
            }
            else
            {
                GameGlue.I.Hint("You don't have enough gold.");
            }
        }
        else if (dialog && Input.GetKeyDown(KeyCode.Alpha2))
        {
            dialog = false;
            GameGlue.I?.Hint("You walk away.");
        }
    }

    void UpdateProximity()
    {
        Transform listener = FindClosestListener();
        bool wasInside = inside;
        inside = listener != null;
        currentListener = listener;

        // Show hint when entering
        if (inside && !wasInside)
        {
            GameGlue.I?.Hint("Press T to talk.");
        }
        // ✅ NEW: Keep showing hint if still inside and not in dialog
        else if (inside && wasInside && !dialog)
        {
            GameGlue.I?.Hint("Press T to talk.");
        }
        // Clear hint when leaving
        else if (!inside && wasInside)
        {
            if (dialog)
            {
                dialog = false;
            }
            GameGlue.I?.Hint(string.Empty);
        }
    }

    Transform FindClosestListener()
    {
        float bestSqr = talkRange * talkRange;
        Transform best = null;
        foreach (var hp in FindObjectsByType<Arthur_WorldHPBar>(FindObjectsSortMode.None))
        {
            if (!hp) continue;
            float sqr = (hp.transform.position - transform.position).sqrMagnitude;
            if (sqr <= bestSqr)
            {
                bestSqr = sqr;
                best = hp.transform;
            }
        }
        return best;
    }
}
