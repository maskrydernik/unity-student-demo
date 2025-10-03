using UnityEngine;

namespace MiniWoW
{
    public enum TargetRule
    {
        Self,
        Friendly,
        Enemy,
        Any
    }

    [CreateAssetMenu(menuName = "MiniWoW/Ability Definition", fileName = "Ability")]
    public class AbilityDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id = "ability.id";
        public string displayName = "Ability";
        public Sprite icon;

        [Header("Targeting")]
        public TargetRule targetRule = TargetRule.Enemy;
        public bool requiresTarget = true;
        public float range = 25f;

        [Header("Timing")]
        public float castTime = 0f;
        public float cooldown = 5f;
        public bool triggersGCD = false;
        public float gcdSeconds = 1.0f;

        [Header("Effects")]
        public float damage = 0f;
        public float healing = 0f;

        [Header("Projectile")]
        public bool useProjectile = false;
        public GameObject projectilePrefab;
        public bool useTravelTime = true;
        public float projectileTravelTime = 0.8f;
        public float projectileSpeed = 20f; // used if useTravelTime == false
        public bool homing = true;

        [Header("VFX & SFX")]
        public GameObject spawnVFX;
        public GameObject hitVFX;
        public AudioClip sfxCast;
        public AudioClip sfxImpact;

        [Header("Misc")]
        public bool canCastWhileMoving = true;
    }
}
