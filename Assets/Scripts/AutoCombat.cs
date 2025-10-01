// AutoCombat.cs
// Nicholas: RuneScape-like loop. Finds nearest enemy, approaches, swings on interval.
// Kameron: rage burst then exhaust; writes normalized value to UnitExtras.SetRageUI.
// Jordon: if this swing kills, attacker gains speed.

using UnityEngine;
using UnityEngine.AI;

public class AutoCombat : MonoBehaviour
{
    public enum Team { Player, Enemy }
    public Team team = Team.Player;

    [Header("Combat")]
    public float range = 2.0f;
    public float attackInterval = 1.0f;
    public float searchRadius = 8f;

    [Header("Rage/Exhaust (Kameron)")]
    public float rage = 0f;              // 0..1
    public float rageGainPerHit = 0.15f;
    public float rageDuration = 4f;
    public float exhaustDuration = 3f;
    bool raging = false;
    bool exhausted = false;
    float stateTimer = 0f;

    UnitExtras stats;
    NavMeshAgent agent;
    AutoCombat forcedTarget;
    float attackTimer = 0f;

    void Start()
    {
        stats = GetComponent<UnitExtras>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (raging || exhausted)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                if (raging) { raging = false; exhausted = true; stateTimer = exhaustDuration; }
                else        { exhausted = false; }
            }
        }

        stats.SetRageUI(raging ? 1f : (exhausted ? 0f : rage));

        attackTimer -= Time.deltaTime;

        AutoCombat target = forcedTarget;
        if (target == null) target = FindNearestEnemy();

        if (target != null)
        {
            Vector3 tp = target.transform.position;
            float dist = Vector3.Distance(transform.position, tp);

            if (dist > range) agent.SetDestination(tp);

            if (attackTimer <= 0f && dist <= range)
            {
                attackTimer = attackInterval;

                float dmg = stats.GetDamage();
                if (raging)   dmg *= 1.8f;
                if (exhausted) dmg *= 0.7f;

                UnitExtras tStats = target.GetComponent<UnitExtras>();
                float preHP = tStats.hp;
                float reduced = dmg * tStats.GetArmorFactor();
                tStats.TakeDamage(reduced);

                if (preHP > 0f && tStats.hp <= 0f) stats.GainSpeedOnKill();

                if (!raging && !exhausted)
                {
                    rage = Mathf.Clamp01(rage + rageGainPerHit);
                    if (rage >= 1f) { raging = true; stateTimer = rageDuration; rage = 0f; }
                }
            }
        }
    }

    public void SetForcedTarget(AutoCombat t) { forcedTarget = t; }

    AutoCombat FindNearestEnemy()
    {
        AutoCombat best = null; float bestD = Mathf.Infinity;
        foreach (AutoCombat ac in FindObjectsOfType<AutoCombat>())
        {
            if (ac != this && ac.team != this.team)
            {
                float d = Vector3.Distance(transform.position, ac.transform.position);
                if (d <= searchRadius && d < bestD) { bestD = d; best = ac; }
            }
        }
        return best;
    }
}
