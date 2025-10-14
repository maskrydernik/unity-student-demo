using UnityEngine;

namespace MiniWoW
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Targetable))]
    public class TrainingDummy : MonoBehaviour
    {
        [Header("Setup")]
        public string dummyName = "Training Dummy";
        public Faction faction = Faction.Enemy;
        public float maxHP = 1000f;

        private void Awake()
        {
            var h = GetComponent<Health>();
            h.SetFaction(faction);
            h.SetMax(maxHP, true);

            var t = GetComponent<Targetable>();
            var label = dummyName + (faction == Faction.Enemy ? " [Enemy]" : " [Friendly]");
            var field = typeof(Targetable).GetField("displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(t, label);
        }
    }
}
