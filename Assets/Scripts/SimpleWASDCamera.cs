// SimpleWASDCamera.cs
// Utility component. Not credited to a person.
using UnityEngine;

public class SimpleWASDCamera : MonoBehaviour
{
    void Update()
    {
        float speed = 12f;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= 2f;
        }

        float movementStep = speed * Time.deltaTime;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            direction += forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction -= forward;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += right;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction -= right;
        }

        transform.position += direction * movementStep;
    }
}
