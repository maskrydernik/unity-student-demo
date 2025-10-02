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

    float attackTimer;
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
        attackTimer -= Time.deltaTime;

        Nicholas_AutoCombat target = FindNearestEnemy();
        if (target == null)
        {
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget > range && agent != null)
        {
            agent.SetDestination(target.transform.position);
        }

        if (attackTimer > 0f || distanceToTarget > range)
        {
            return;
        }

        attackTimer = attackInterval;
        float damage = gear ? gear.GetDamage() : 10f;
        if (rage != null)
        {
            damage = rage.ComputeOutgoingDamage(damage);
        }

        var targetHealth = target.GetComponent<Arthur_WorldHPBar>();
        float previousHealth = targetHealth ? targetHealth.hp : 0f;

        float armorFactor = 1f;
        var targetGear = target.GetComponent<Steven_GearStats>();
        if (targetGear != null)
        {
            armorFactor = targetGear.GetArmorFactor();
        }

        float finalDamage = damage * armorFactor;
        if (targetHealth != null)
        {
            targetHealth.ApplyDamage(finalDamage);
        }

        if (previousHealth > 0f && targetHealth != null && targetHealth.hp <= 0f && speedOnKill != null)
        {
            speedOnKill.Gain();
        }

        if (rage != null)
        {
            rage.NotifyHit();
        }
    }

    Nicholas_AutoCombat FindNearestEnemy()
    {
        Nicholas_AutoCombat best = null;
        float bestDistance = Mathf.Infinity;

        foreach (var autoCombat in FindObjectsByType<Nicholas_AutoCombat>(FindObjectsSortMode.None))
        {
            if (autoCombat == this || autoCombat.team == team)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, autoCombat.transform.position);
            if (distance <= searchRadius && distance < bestDistance)
            {
                bestDistance = distance;
                best = autoCombat;
            }
        }
        return best;
    }
}
