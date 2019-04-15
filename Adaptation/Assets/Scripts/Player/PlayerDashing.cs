using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashing : MonoBehaviour
{
    public bool Dashing { get; private set; }

    float coolDown = 2f;
    float timer;

    float dashSpeed = 25f;
    float dashDistance = 10f;

    Vector3 posBefore;
    Vector3 direction;

    Vector3 lastFramePos;

    private void Awake()
    {
        timer = coolDown;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (Input.GetButton("Dash") && timer >= coolDown && !Dashing)
        {
            ActivateDash();
            Debug.Log("Dashing");
        }

        if (Dashing)
        {
            if (Vector3.Distance(transform.position, posBefore) >= dashDistance
                || lastFramePos == transform.position)
            {
                Dashing = false;
                timer = 0;
                Debug.Log("Stopped dashing");
            }
        }
    }

    private void FixedUpdate()
    {
        if (Dashing)
        {
            lastFramePos = transform.position;
            transform.position += direction.normalized * dashSpeed * Time.deltaTime;
        }
    }

    private void ActivateDash()
    {
        posBefore = transform.position;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal == 0 && vertical == 0)
            direction = transform.forward;
        else
            direction.Set(horizontal, 0f, vertical);

        Dashing = true;
    }
}
