using UnityEngine;

namespace MiniWoW
{
    [DisallowMultipleComponent]
    public class Targetable : MonoBehaviour
    {
        [SerializeField] private string displayName = "Unit";
        [SerializeField] private Transform aimPoint;
        [SerializeField] private Health health;

        private Renderer[] renderers;
        private Color[] originalColors;
        private bool selected = false;

        public string DisplayName => displayName;
        public Health Health => health;
        public Transform AimPoint => aimPoint != null ? aimPoint : transform;
        public Faction Faction => health != null ? health.Faction : Faction.Enemy;

        private void Reset()
        {
            health = GetComponent<Health>();
        }

        private void Awake()
        {
            if (!health) health = GetComponent<Health>();
            renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers != null)
            {
                originalColors = new Color[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    var mat = renderers[i].material;
                    originalColors[i] = mat.HasProperty("_Color") ? mat.color : Color.white;
                }
            }
        }

        public void SetSelected(bool value)
        {
            if (selected == value) return;
            selected = value;
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                var mat = renderers[i].material;
                if (!mat.HasProperty("_Color")) continue;
                mat.color = value ? Color.yellow : originalColors[i];
            }
        }
    }
}
