using System;
using UnityEngine;

namespace MiniWoW
{
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private Faction faction = Faction.Enemy;

        public float Max => maxHealth;
        public float Current { get; private set; }
        public bool IsDead => Current <= 0f;
        public Faction Faction => faction;

        public event Action<float, float> OnHealthChanged; // current, max
        public event Action OnDied;

        private void Awake()
        {
            Current = Mathf.Max(1f, maxHealth);
        }

        public void SetFaction(Faction f) => faction = f;

        public void SetMax(float value, bool fullHeal = true)
        {
            maxHealth = Mathf.Max(1f, value);
            if (fullHeal) Current = maxHealth;
            OnHealthChanged?.Invoke(Current, maxHealth);
        }

        public void Damage(float amount, GameObject source = null)
        {
            if (IsDead) return;
            if (amount <= 0f) return;
            Current = Mathf.Max(0f, Current - amount);
            OnHealthChanged?.Invoke(Current, maxHealth);
            if (IsDead) OnDied?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            if (amount <= 0f) return;
            Current = Mathf.Min(maxHealth, Current + amount);
            OnHealthChanged?.Invoke(Current, maxHealth);
        }
    }
}
