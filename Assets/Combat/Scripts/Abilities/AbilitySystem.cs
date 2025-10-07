using System.Collections.Generic;
using UnityEngine;

namespace MiniWoW
{
    [DisallowMultipleComponent]
    public class AbilitySystem : MonoBehaviour
    {
        [Tooltip("Five ability slots. Populate in inspector or via Setup Wizard.")]
        public AbilityDefinition[] loadout = new AbilityDefinition[5];

        public TargetingSystem targeting;
        public Transform projectileSpawn;
        public AudioSource audioSource;

        private Dictionary<AbilityDefinition, float> lastCastTime = new Dictionary<AbilityDefinition, float>();
        private float lastGCDTime = -999f;

        private Health self;

        private void Awake()
        {
            self = GetComponent<Health>();
            if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) { Debug.Log("[AbilitySystem] Key 1 pressed"); TryCastSlot(0); }
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) { Debug.Log("[AbilitySystem] Key 2 pressed"); TryCastSlot(1); }
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) { Debug.Log("[AbilitySystem] Key 3 pressed"); TryCastSlot(2); }
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) { Debug.Log("[AbilitySystem] Key 4 pressed"); TryCastSlot(3); }
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) { Debug.Log("[AbilitySystem] Key 5 pressed"); TryCastSlot(4); }
        }

        public float GetCooldownRemaining(AbilityDefinition def)
        {
            if (def == null) return 0f;
            if (!lastCastTime.TryGetValue(def, out var t)) return 0f;
            float dt = Time.time - t;
            float rem = Mathf.Clamp(def.cooldown - dt, 0f, def.cooldown);
            return rem;
        }

        public bool OnGlobalCooldown() => Time.time - lastGCDTime < 0f ? false : (Time.time - lastGCDTime) < GetCurrentGCD();

        private float GetCurrentGCD() => 1.0f; // simple

        public void TryCastSlot(int index)
        {
            if (index < 0 || index >= loadout.Length) return;
            var def = loadout[index];
            if (!def) return;
            TryCast(def);
        }

        public void TryCast(AbilityDefinition def)
        {
            if (!def) { Debug.Log("[AbilitySystem] No ability definition"); return; }
            if (OnGlobalCooldown() && def.triggersGCD) { Debug.Log($"[AbilitySystem] {def.displayName} on GCD"); return; }
            if (GetCooldownRemaining(def) > 0f) { Debug.Log($"[AbilitySystem] {def.displayName} on cooldown: {GetCooldownRemaining(def):F1}s"); return; }

            Targetable target = ResolveTarget(def);

            if (def.requiresTarget && target == null) { Debug.Log($"[AbilitySystem] {def.displayName} requires target"); return; }

            if (target != null && !ValidateTargetRule(def, target)) { Debug.Log($"[AbilitySystem] {def.displayName} invalid target faction"); return; }

            if (target != null && !InRange(def, target)) { Debug.Log($"[AbilitySystem] {def.displayName} out of range"); return; }

            // Cast time ignored for simplicity; can be added with coroutine.
            Debug.Log($"[AbilitySystem] Casting {def.displayName} on {(target ? target.DisplayName : "self")}");
            Execute(def, target);
            lastCastTime[def] = Time.time;
            if (def.triggersGCD) lastGCDTime = Time.time;
        }

        private Targetable ResolveTarget(AbilityDefinition def)
        {
            if (def.targetRule == TargetRule.Self) return GetComponent<Targetable>();
            if (!targeting) targeting = GetComponent<TargetingSystem>();
            return targeting ? targeting.Current : null;
        }

        private bool ValidateTargetRule(AbilityDefinition def, Targetable target)
        {
            var myFaction = self ? self.Faction : Faction.Player;
            if (def.targetRule == TargetRule.Any) return true;
            if (def.targetRule == TargetRule.Friendly) return FactionRules.IsFriendly(myFaction, target.Faction);
            if (def.targetRule == TargetRule.Enemy) return FactionRules.IsHostile(myFaction, target.Faction);
            if (def.targetRule == TargetRule.Self) return target == GetComponent<Targetable>();
            return false;
        }

        private bool InRange(AbilityDefinition def, Targetable target)
        {
            if (!target) return true;
            float dist = Vector3.Distance(transform.position, target.AimPoint.position);
            return dist <= def.range + 0.1f;
        }

        private void Execute(AbilityDefinition def, Targetable target)
        {
            if (def.sfxCast && audioSource) audioSource.PlayOneShot(def.sfxCast);

            if (def.useProjectile && def.projectilePrefab)
            {
                Transform spawn = projectileSpawn ? projectileSpawn : transform;
                GameObject go = Instantiate(def.projectilePrefab, spawn.position + transform.forward * 0.5f + Vector3.up * 1.2f, spawn.rotation);
                var proj = go.GetComponent<Projectile>();
                if (!proj) proj = go.AddComponent<Projectile>();
                proj.Init(target ? target.AimPoint : transform, def.projectileSpeed, def.projectileTravelTime, def.useTravelTime, def.homing, () =>
                {
                    ApplyEffects(def, target);
                });
            }
            else
            {
                ApplyEffects(def, target);
            }
        }

        private void ApplyEffects(AbilityDefinition def, Targetable target)
        {
            if (target && def.damage > 0f)
            {
                // damage only if hostile or Any
                bool ok = def.targetRule == TargetRule.Any || FactionRules.IsHostile(self ? self.Faction : Faction.Player, target.Faction);
                if (ok)
                {
                    target.Health?.Damage(def.damage, gameObject);
                    // Show damage number
                    FloatingText.Create(target.AimPoint.position, $"-{def.damage:F0}", Color.red);
                }
            }

            if (target && def.healing > 0f)
            {
                // heal only if friendly or Any
                bool ok = def.targetRule == TargetRule.Any || FactionRules.IsFriendly(self ? self.Faction : Faction.Player, target.Faction);
                if (ok)
                {
                    target.Health?.Heal(def.healing);
                    // Show healing number
                    FloatingText.Create(target.AimPoint.position, $"+{def.healing:F0}", Color.green);
                }
            }

            if (def.sfxImpact && audioSource) audioSource.PlayOneShot(def.sfxImpact);
            if (def.hitVFX && target) Instantiate(def.hitVFX, target.AimPoint.position, Quaternion.identity);
        }
    }
}
