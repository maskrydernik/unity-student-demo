// SimpleWASDCamera.cs
// Utility component. Not credited to a person.
using UnityEngine;

public class SimpleWASDCamera : MonoBehaviour
{
    [Tooltip("Units per second while moving without sprinting.")]
    public float moveSpeed = 12f;

    [Tooltip("Multiplier applied to move speed while Left Shift is held.")]
    public float sprintMultiplier = 2f;

    void Update()
    {

        //  float scroll = Input.GetAxis("Mouse ScrollWheel");

        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= sprintMultiplier;
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) direction += forward;
        if (Input.GetKey(KeyCode.S)) direction -= forward;
        if (Input.GetKey(KeyCode.D)) direction += right;
        if (Input.GetKey(KeyCode.A)) direction -= right;

        if (direction.sqrMagnitude > 0f)
        {
            direction.Normalize();
            transform.position += direction * currentSpeed * Time.deltaTime;
        }
    }
}
