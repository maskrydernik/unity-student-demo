using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniWoW
{
    [DisallowMultipleComponent]
    public class CameraController : MonoBehaviour
    {
        [Header("Orbit")]
        public float distance = 6f;
        public float minDistance = 2f;
        public float maxDistance = 12f;
        public float orbitSensitivity = 120f;
        public float zoomSensitivity = 2f;
        public float minPitch = -30f;
        public float maxPitch = 75f;

        private Transform target;
        private PlayerMotor player;
        private float yaw;
        private float pitch = 15f;
        private Transform cam;

        public Transform CameraTransform => transform;

        public void SetTarget(Transform t, PlayerMotor p)
        {
            target = t;
            player = p;
            if (target) yaw = target.eulerAngles.y;
        }

        private void Awake()
        {
            cam = transform;
        }

        private void LateUpdate()
        {
            if (!target) return;

            HandleOrbitAndZoom();
            PositionCamera();
        }

        private void HandleOrbitAndZoom()
        {
            // Left mouse: orbit around target (do not rotate character)
            if (Input.GetMouseButton(0) && !IsPointerOverUI())
            {
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");
                yaw += mx * orbitSensitivity * Time.unscaledDeltaTime;
                pitch -= my * orbitSensitivity * Time.unscaledDeltaTime;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            // Right mouse: rotate player already handled in PlayerMotor; here just pitch
            if (Input.GetMouseButton(1) && !IsPointerOverUI())
            {
                float my = Input.GetAxis("Mouse Y");
                pitch -= my * orbitSensitivity * Time.unscaledDeltaTime;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            // Zoom
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance = Mathf.Clamp(distance - scroll * zoomSensitivity, minDistance, maxDistance);
            }

            // Align yaw with player if RMB held
            if (Input.GetMouseButton(1) && player != null)
            {
                yaw = player.transform.eulerAngles.y;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void PositionCamera()
        {
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -distance);
            Vector3 targetPos = target.position + Vector3.up * 1.6f;
            cam.position = targetPos + offset;
            cam.rotation = rot;
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
