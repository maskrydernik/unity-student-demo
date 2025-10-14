using UnityEngine;

public class SimpleMover : MonoBehaviour
{
    public float speed = 5f;
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, y, 0);
        transform.Translate(move * speed * Time.deltaTime);

        bool isMoving = move.magnitude > 0;
        anim.SetBool("isWalking", isMoving);
    }
}
