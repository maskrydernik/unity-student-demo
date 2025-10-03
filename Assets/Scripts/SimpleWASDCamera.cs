// Simple camera. WASD move, mouse wheel zoom with ground-anchor preservation.
// Arrow keys: Left/Right = yaw, Up/Down = pitch. Ctrl + Arrow = pan.
// Edge scroll disabled by default.

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleWASDCamera : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 12f;
    public float sprintMultiplier = 2f;
    public bool edgeScroll = false; // off by default
    public int edgeBorder = 12;

    [Header("Rotate/Tilt (Arrow Keys)")]
    public float yawSpeed = 90f;    // deg/s
    public float pitchSpeed = 60f;  // deg/s
    public Vector2 pitchLimits = new Vector2(20f, 80f);

    [Header("Zoom")]
    public float zoomStep = 6f;         // units per scroll tick
    public float zoomSmoothTime = 0.08f;
    public float minHeight = 6f;
    public float maxHeight = 60f;

    [Header("Ground")]
    public float groundY = 0f;

    [Header("Bounds (optional)")]
    public bool clampXZ = false;
    public Bounds xzBounds = new Bounds(Vector3.zero, new Vector3(200, 0, 200));

    Camera cam;
    float targetHeight;
    float zoomVel;
    Plane groundPlane;

    void Awake()
    {
        cam = GetComponent<Camera>();
        groundPlane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
        targetHeight = Mathf.Clamp(transform.position.y - groundY, minHeight, maxHeight);

        var e = transform.eulerAngles;
        e.x = ClampPitch(e.x);
        transform.eulerAngles = e;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // WASD planar move (ignores pitch)
        float speed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = transform.right; right.y = 0f; right.Normalize();

        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += fwd;
        if (Input.GetKey(KeyCode.S)) move -= fwd;
        if (Input.GetKey(KeyCode.D)) move += right;
        if (Input.GetKey(KeyCode.A)) move -= right;

        if (edgeScroll)
        {
            Vector3 m = Input.mousePosition;
            if (m.x <= edgeBorder) move -= right;
            else if (m.x >= Screen.width - edgeBorder) move += right;
            if (m.y <= edgeBorder) move -= fwd;
            else if (m.y >= Screen.height - edgeBorder) move += fwd;
        }

        if (move.sqrMagnitude > 0f)
        {
            move.Normalize();
            transform.position += move * speed * dt;
        }

        // Arrow keys: yaw and pitch
        Vector3 e = transform.eulerAngles;
        if (Input.GetKey(KeyCode.LeftArrow)) e.y -= yawSpeed * dt;
        if (Input.GetKey(KeyCode.RightArrow)) e.y += yawSpeed * dt;

        float pitch = NormalizePitch(e.x);
        if (Input.GetKey(KeyCode.UpArrow)) pitch -= pitchSpeed * dt;
        if (Input.GetKey(KeyCode.DownArrow)) pitch += pitchSpeed * dt;
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
        e.x = pitch;
        transform.eulerAngles = e;

        // Zoom with ground-anchor preservation under current cursor
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0f)
            targetHeight = Mathf.Clamp(targetHeight - scroll * zoomStep, minHeight, maxHeight);

        float currentHeight = transform.position.y - groundY;
        if (!Mathf.Approximately(currentHeight, targetHeight))
        {
            float newHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref zoomVel, zoomSmoothTime);
            float dh = newHeight - currentHeight;

            Vector3 pos = transform.position + new Vector3(0f, dh, 0f);
            Vector3 corrected = ReanchorToGroundUnderCursor(pos, Input.mousePosition);
            transform.position = corrected;
        }

        // Bounds clamp
        if (clampXZ)
        {
            Vector3 p = transform.position;
            Vector3 c = xzBounds.center;
            Vector3 ex = xzBounds.extents;
            p.x = Mathf.Clamp(p.x, c.x - ex.x, c.x + ex.x);
            p.z = Mathf.Clamp(p.z, c.z - ex.z, c.z + ex.z);
            transform.position = p;
        }
    }

    // Keep the ground point under the specified screen point stable after moving to newPos.
    Vector3 ReanchorToGroundUnderCursor(Vector3 newPos, Vector3 screenPoint)
    {
        // Ground point before zoom
        Ray rOld = cam.ScreenPointToRay(screenPoint);
        Vector3 gOld = WorldOnGround(rOld, transform.position);

        // Ground point after zoom (temporarily place camera)
        Vector3 saved = transform.position;
        transform.position = newPos;
        Ray rNew = cam.ScreenPointToRay(screenPoint);
        Vector3 gNew = WorldOnGround(rNew, newPos);
        transform.position = saved;

        // Shift by delta so the same ground point remains under the cursor
        Vector3 delta = gOld - gNew;
        newPos += delta;

        // Enforce height clamp exactly
        float h = Mathf.Clamp(newPos.y - groundY, minHeight, maxHeight);
        newPos.y = groundY + h;
        return newPos;
    }

    // Ray/plane intersect; fallback to vertical drop if parallel.
    Vector3 WorldOnGround(Ray ray, Vector3 fallbackFrom)
    {
        if (groundPlane.Raycast(ray, out float t))
            return ray.GetPoint(t);
        Vector3 p = fallbackFrom; p.y = groundY; return p;
    }

    static float NormalizePitch(float xDeg)
    {
        xDeg = Mathf.Repeat(xDeg, 360f);
        return (xDeg <= 180f) ? xDeg : xDeg - 360f;
    }

    float ClampPitch(float xDeg)
    {
        float p = NormalizePitch(xDeg);
        return Mathf.Clamp(p, pitchLimits.x, pitchLimits.y);
    }
}
