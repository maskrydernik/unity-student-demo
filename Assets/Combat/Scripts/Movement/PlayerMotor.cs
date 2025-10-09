using UnityEngine;

namespace MiniWoW
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;
        public float rotateSpeed = 180f;
        public float jumpHeight = 1.2f;
        public float gravity = -20f;

        [Header("References")]
        public Transform cameraPivot; // target for camera orbit
        public CameraController cameraController;

        private CharacterController controller;
        private Vector3 velocity;
        private Transform cam;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (cameraController != null)
            {
                cam = cameraController.CameraTransform;
                if (!cameraPivot) cameraPivot = transform;
                cameraController.SetTarget(cameraPivot, this);
            }
            else
            {
                cam = Camera.main ? Camera.main.transform : null;
            }
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            // Rotate character with RMB
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                transform.Rotate(0f, mouseX * rotateSpeed * Time.deltaTime, 0f);
            }

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 move = Vector3.zero;
            if (cam != null)
            {
                Vector3 forward = cam.forward; forward.y = 0f; forward.Normalize();
                Vector3 right = cam.right; right.y = 0f; right.Normalize();
                move = forward * input.y + right * input.x;
            }
            else
            {
                move = transform.forward * input.y + transform.right * input.x;
            }

            controller.Move(move * moveSpeed * Time.deltaTime);

            if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;

            if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
