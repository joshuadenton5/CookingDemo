using UnityEngine;

public class PlayerController : MonoBehaviour //allows for smooth movement, less jitters when colliding with other colliders
{
    public float speed;
    private bool allowMove = true;
    private Rigidbody rb;
    private Vector3 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (allowMove)
        {
           // float translation = Input.GetAxisRaw("Vertical"); 
            //float straffe = Input.GetAxisRaw("Horizontal");
            //moveDirection = (straffe * transform.right + translation * transform.forward).normalized;
        }
    }

    void FixedUpdate()
    {
        if (allowMove)
            Move();
    }

    void Move()
    {
        Vector3 y = new Vector3(0, rb.velocity.y, 0); //accounting for the velocity in the y axis
        rb.velocity = moveDirection * speed * Time.deltaTime;
        rb.velocity += y;
    }

    public void AllowMove(bool m)
    {
        allowMove = m;
    } 

}
