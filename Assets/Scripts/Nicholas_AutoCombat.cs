// Nicholas_AutoCombat.cs
// Finds nearest enemy, moves, attacks on interval. No rage here.
using UnityEngine;
using UnityEngine.AI;

public class Nicholas_AutoCombat : MonoBehaviour
{
    public enum Team { Player, Enemy }
    public Team team = Team.Player;
    public float range = 2f;
    public float attackInterval = 1f;
    public float searchRadius = 8f;

    float atkTimer;
    NavMeshAgent agent;
    Steven_GearStats gear;
    Arthur_WorldHPBar hp;
    Kameron_RageModule rage; // optional
    Jordon_SpeedOnKill speedOnKill; // optional

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        gear = GetComponent<Steven_GearStats>();
        hp   = GetComponent<Arthur_WorldHPBar>();
        rage = GetComponent<Kameron_RageModule>();
        speedOnKill = GetComponent<Jordon_SpeedOnKill>();
    }

    void Update()
    {
        atkTimer -= Time.deltaTime;
        var t = FindNearestEnemy();
        if (t == null) return;

        float dist = Vector3.Distance(transform.position, t.transform.position);
        if (dist > range) agent.SetDestination(t.transform.position);

        if (atkTimer <= 0f && dist <= range)
        {
            atkTimer = attackInterval;
            float dmg = gear ? gear.GetDamage() : 10f;
            if (rage) dmg = rage.ComputeOutgoingDamage(dmg);

            var thp = t.GetComponent<Arthur_WorldHPBar>();
            float pre = thp ? thp.hp : 0f;

            float reduced = dmg * (t.GetComponent<Steven_GearStats>() ? t.GetComponent<Steven_GearStats>().GetArmorFactor() : 1f);
            if (thp) thp.ApplyDamage(reduced);

            if (pre > 0 && thp && thp.hp <= 0 && speedOnKill) speedOnKill.Gain();
            if (rage) rage.NotifyHit();
        }
    }

    Nicholas_AutoCombat FindNearestEnemy()
    {
        Nicholas_AutoCombat best = null; float bestD = Mathf.Infinity;
        foreach (var ac in FindObjectsByType<Nicholas_AutoCombat>(FindObjectsSortMode.None))
        {
            if (ac != this && ac.team != team)
            {
                float d = Vector3.Distance(transform.position, ac.transform.position);
                if (d <= searchRadius && d < bestD){ bestD = d; best = ac; }
            }
        }
        return best;
    }
}
