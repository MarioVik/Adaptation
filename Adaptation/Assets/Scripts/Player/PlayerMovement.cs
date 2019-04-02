using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;

    Vector3 movement;
    Animator anim;
    Rigidbody playerRigidBody;
    int floorMask;
    float camRayLength = 100f;

    bool dashEnabled;

    public void IncreaseSpeed(float increase)
    {
        speed += increase;
    }

    private void Awake()
    {
        floorMask = LayerMask.GetMask("Floor");
        anim = GetComponent<Animator>();
        playerRigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float horisontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Move(horisontal, vertical);
        Turning();
        Animating(horisontal, vertical);
    }

    private void Move(float horizontal, float vertical)
    {
        movement.Set(horizontal, 0f, vertical);
        movement = movement.normalized * speed * Time.deltaTime;
        playerRigidBody.MovePosition(transform.position + movement);
    }

    private void Turning()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorHit;

        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
        {
            Vector3 playerToMouse = floorHit.point - transform.position;
            playerToMouse.y = 0f;

            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
            playerRigidBody.MoveRotation(newRotation);
        }
    }

    private void Animating(float h, float v)
    {
        bool walking = h != 0f || v != 0f;
        anim.SetBool("IsWalking", walking);
    }

    public void EnableDash()
    {
        dashEnabled = false;
    }

    private void Dash()
    {
        if (dashEnabled)
        {
            //Move a set distance in turned direction 
        }
    }
}
