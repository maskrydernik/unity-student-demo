// Kameron_RageModule.cs
// Fills on hit. When full, short rage buff, then short exhaust debuff. Updates a fill amount via Arthur_WorldHPBar optional Rage image.
using UnityEngine;

public class Kameron_RageModule : MonoBehaviour
{
    public float rage = 0f;           // 0..1
    public float gainPerHit = 0.15f;
    public float rageDur = 4f;
    public float exhaustDur = 3f;
    bool raging = false, exhausted = false;
    float timer = 0f;

    public float ComputeOutgoingDamage(float baseDmg)
    {
        float d = baseDmg;
        if (raging) d *= 1.8f;
        if (exhausted) d *= 0.7f;
        UpdateRageUI();
        return d;
    }

    public void NotifyHit()
    {
        if (!raging && !exhausted)
        {
            rage = Mathf.Clamp01(rage + gainPerHit);
            if (rage >= 1f){ raging = true; timer = rageDur; rage = 0f; }
        }
        Tick(Time.deltaTime);
    }

    void Update(){ Tick(Time.deltaTime); }

    void Tick(float dt)
    {
        if (!raging && !exhausted) return;
        timer -= dt;
        if (timer <= 0f)
        {
            if (raging){ raging = false; exhausted = true; timer = exhaustDur; }
            else { exhausted = false; }
        }
        UpdateRageUI();
    }

    void UpdateRageUI()
    {
        var hp = GetComponent<Arthur_WorldHPBar>();
        if (!hp) return;
        float val = raging ? 1f : (exhausted ? 0f : rage);
        hp.SetRageFill(val);
    }
}
