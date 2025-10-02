// Arthur_WorldHPBar.cs
// Tracks HP and draws a simple world-space bar. Optional Rage fill as child Image named "Rage".
using UnityEngine;
using UnityEngine.UI;

public class Arthur_WorldHPBar : MonoBehaviour
{
    public Canvas worldCanvasPrefab;
    public float maxHP = 100f;
    public float hp = 100f;
    public float uiHeight = 2f;

    Image hpImage;
    Image rageImage;

    void Start()
    {
        if (worldCanvasPrefab)
        {
            Canvas c = Instantiate(worldCanvasPrefab, transform);
            c.transform.localPosition = new Vector3(0, uiHeight, 0);
            foreach (var img in c.GetComponentsInChildren<Image>())
            {
                if (img.name == "HP") hpImage = img;
                if (img.name == "Rage") rageImage = img;
            }
            Sync();
        }
    }

    public void ApplyDamage(float dmg)
    {
        hp = Mathf.Max(0, hp - dmg);
        Sync();
        if (hp <= 0) Die();
    }

    public void Sync(){ if (hpImage) hpImage.fillAmount = maxHP > 0 ? hp/maxHP : 0f; }

    public void SetRageFill(float v){ if (rageImage) rageImage.fillAmount = Mathf.Clamp01(v); }

    void Die()
    {
        // Quest notify and simple drop are handled by other scripts on this object, if present.
        var ac = GetComponent<Nicholas_AutoCombat>();
        if (ac != null && ac.team == Nicholas_AutoCombat.Team.Enemy)
        {
            if (Christopher_RaiderQuest.I) Christopher_RaiderQuest.I.NotifyEnemyDeath(gameObject);
        }
        Destroy(gameObject);
    }
}
