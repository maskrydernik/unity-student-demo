// SimpleWASDCamera.cs
// Utility component. Not credited to a person.
using UnityEngine;

public class SimpleWASDCamera : MonoBehaviour
{
    void Update()
    {
        float s = 12f * (Input.GetKey(KeyCode.LeftShift) ? 2f : 1f) * Time.deltaTime;
        Vector3 f = transform.forward; f.y = 0; f.Normalize();
        Vector3 r = transform.right; r.y = 0; r.Normalize();
        Vector3 d = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) d += f;
        if (Input.GetKey(KeyCode.S)) d -= f;
        if (Input.GetKey(KeyCode.D)) d += r;
        if (Input.GetKey(KeyCode.A)) d -= r;
        transform.position += d * s;
    }
}
