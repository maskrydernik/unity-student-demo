// David_ArcherTower.cs
// Fires straight-flying arrows at the nearest Nicholas_AutoCombat enemy, no Rigidbody or gear dependencies.

using UnityEngine;

public class David_ArcherTower : MonoBehaviour
{
    public enum Team { Player, Enemy }
    public Team team = Team.Player;

    [Header("Archer Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float attackRange = 10f;
    public float attackInterval = 1.5f;
    public float arrowSpeed = 25f;
    public float baseDamage = 10f; // damage per arrow
    public float projectileLifetime = 6f;

    float attackTimer;
    Kameron_RageModule rage;

    void Start()
    {
        rage = GetComponent<Kameron_RageModule>();
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;

        Nicholas_AutoCombat target = FindNearestEnemy();
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > attackRange) return;

        // Face the target
        Vector3 direction = (target.transform.position - firePoint.position).normalized;
        firePoint.rotation = Quaternion.LookRotation(direction);

        if (attackTimer <= 0f)
        {
            attackTimer = attackInterval;
            FireArrow(target);
        }
    }

    void FireArrow(Nicholas_AutoCombat target)
    {
        if (arrowPrefab == null || firePoint == null) return;

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);

        // Initialize arrow with target and damage
        David_Arrow arrowScript = arrow.GetComponent<David_Arrow>();
        if (arrowScript != null)
        {
            float dmg = ComputeDamage();
            arrowScript.Initialize(this, target, dmg, arrowSpeed);
        }

        Destroy(arrow, projectileLifetime);
    }

    public float ComputeDamage()
    {
        float damage = baseDamage;

        // Optionally apply rage multiplier
        if (rage != null)
            damage = rage.ComputeOutgoingDamage(damage);

        return damage;
    }

    Nicholas_AutoCombat FindNearestEnemy()
    {
        Nicholas_AutoCombat best = null;
        float bestDist = Mathf.Infinity;

        foreach (var autoCombat in FindObjectsByType<Nicholas_AutoCombat>(FindObjectsSortMode.None))
        {
            if (autoCombat == null || autoCombat.team.ToString() == team.ToString())
                continue;

            float dist = Vector3.Distance(transform.position, autoCombat.transform.position);
            if (dist < bestDist && dist <= attackRange)
            {
                bestDist = dist;
                best = autoCombat;
            }
        }

        return best;
    }
}
