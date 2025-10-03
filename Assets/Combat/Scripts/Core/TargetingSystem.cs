using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniWoW
{
    [DisallowMultipleComponent]
    public class TargetingSystem : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private float clickMaxMovePixels = 5f;
        [SerializeField] private float clickMaxTime = 0.25f;

        public Targetable Current { get; private set; }

        private Vector3 downPos;
        private float downTime;

        private void Start()
        {
            if (!cam) cam = Camera.main;
            if (!cam)
            {
                Debug.LogError("[TargetingSystem] No camera found! Targeting will not work.");
            }
            else
            {
                Debug.Log($"[TargetingSystem] Initialized with camera: {cam.name}");
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                downPos = Input.mousePosition;
                downTime = Time.unscaledTime;
                Debug.Log($"[TargetingSystem] Mouse down at {downPos}");
            }
            if (Input.GetMouseButtonUp(0))
            {
                float dist = Vector2.Distance(downPos, Input.mousePosition);
                float t = Time.unscaledTime - downTime;
                bool overUI = IsPointerOverUI();
                Debug.Log($"[TargetingSystem] Mouse up - Distance: {dist:F2}, Time: {t:F3}, Over UI: {overUI}");
                
                if (dist <= clickMaxMovePixels && t <= clickMaxTime && !overUI)
                {
                    TrySelectUnderCursor();
                }
                else
                {
                    if (dist > clickMaxMovePixels) Debug.Log($"[TargetingSystem] Click rejected: moved too far ({dist:F2} > {clickMaxMovePixels})");
                    if (t > clickMaxTime) Debug.Log($"[TargetingSystem] Click rejected: took too long ({t:F3} > {clickMaxTime})");
                    if (overUI) Debug.Log("[TargetingSystem] Click rejected: pointer over UI");
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetTarget(null);
            }
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void TrySelectUnderCursor()
        {
            if (!cam)
            {
                Debug.LogWarning("[TargetingSystem] No camera assigned!");
                return;
            }
            
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.Log($"[TargetingSystem] Raycasting from {Input.mousePosition} with ray: {ray.origin} -> {ray.direction}");
            
            if (Physics.Raycast(ray, out var hit, 200f, targetMask, QueryTriggerInteraction.Ignore))
            {
                Debug.Log($"[TargetingSystem] Hit: {hit.collider.gameObject.name} at {hit.point}");
                var t = hit.collider.GetComponentInParent<Targetable>();
                if (t != null)
                {
                    Debug.Log($"[TargetingSystem] Found Targetable: {t.DisplayName}");
                    SetTarget(t);
                }
                else
                {
                    Debug.LogWarning($"[TargetingSystem] Hit object '{hit.collider.gameObject.name}' has no Targetable component!");
                }
            }
            else
            {
                Debug.Log($"[TargetingSystem] Raycast hit nothing (Layer mask: {targetMask.value})");
            }
        }

        public void SetTarget(Targetable t)
        {
            if (Current == t) return;
            if (Current)
            {
                Current.SetSelected(false);
                Debug.Log($"[TargetingSystem] Deselected: {Current.DisplayName}");
            }
            Current = t;
            if (Current)
            {
                Current.SetSelected(true);
                Debug.Log($"[TargetingSystem] Selected: {Current.DisplayName} (Faction: {Current.Faction})");
            }
            else
            {
                Debug.Log("[TargetingSystem] Target cleared");
            }
        }
    }
}
