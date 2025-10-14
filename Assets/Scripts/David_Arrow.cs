// David_Arrow.cs
// Handles straight arrow flight toward the assigned target, no Rigidbody or physics.

using UnityEngine;

public class David_Arrow : MonoBehaviour
{
    float damage;
    float speed;
    Nicholas_AutoCombat target;
    David_ArcherTower source;
    bool initialized;

    public void Initialize(David_ArcherTower src, Nicholas_AutoCombat tgt, float dmg, float spd)
    {
        source = src;
        target = tgt;
        damage = dmg;
        speed = spd;
        initialized = true;
    }

    void Update()
    {
        if (!initialized || target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move straight toward target
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);

        // Check if close enough to hit
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance < 0.5f) // hit radius
        {
            ApplyDamage();
        }
    }

    void ApplyDamage()
    {
        if (target == null) return;

        var targetHP = target.GetComponent<Arthur_WorldHPBar>();
        if (targetHP != null)
        {
            targetHP.ApplyDamage(damage);

            // Rage integration
            var rage = source != null ? source.GetComponent<Kameron_RageModule>() : null;
            if (rage != null)
                rage.NotifyHit();
        }

        Destroy(gameObject);
    }
}
