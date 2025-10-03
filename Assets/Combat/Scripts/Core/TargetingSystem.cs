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
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                downPos = Input.mousePosition;
                downTime = Time.unscaledTime;
            }
            if (Input.GetMouseButtonUp(0))
            {
                float dist = Vector2.Distance(downPos, Input.mousePosition);
                float t = Time.unscaledTime - downTime;
                if (dist <= clickMaxMovePixels && t <= clickMaxTime && !IsPointerOverUI())
                {
                    TrySelectUnderCursor();
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
            if (!cam) return;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 200f, targetMask, QueryTriggerInteraction.Ignore))
            {
                var t = hit.collider.GetComponentInParent<Targetable>();
                if (t != null) SetTarget(t);
            }
        }

        public void SetTarget(Targetable t)
        {
            if (Current == t) return;
            if (Current) Current.SetSelected(false);
            Current = t;
            if (Current) Current.SetSelected(true);
        }
    }
}
