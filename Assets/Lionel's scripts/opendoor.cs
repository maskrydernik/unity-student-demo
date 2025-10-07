using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OpenDoor : MonoBehaviour
{
    public GameObject door;
    public Vector3 openRotation = new Vector3(0, -90, 0);

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
                Open();
        }
    }

    void Open()
    {
        if (door != null)
            door.transform.localEulerAngles = openRotation;
    }
}
