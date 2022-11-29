using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public float moveSpeed, dashSpeed, dashTime, dashCooldown, smoothTurnTime;

    Rigidbody rb;
    Transform bean;
    CapsuleCollider beanCollider;
    InputActions input;

    float dashTimer = 0;
    float dashCooldownTimer = 0;

    Vector3 origScale;
    Vector2 inputDir, moveDir, aimDir;
    float targetAngle, smoothAngle, smoothTurnVelocity, _moveSpeed;
    

    void Start()
    {
        input = new InputActions();
        rb = GetComponent<Rigidbody>();
        bean = transform.GetChild(0);
        origScale = bean.localScale;
        _moveSpeed = moveSpeed;
        beanCollider = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        Directional();
        if(dashTimer <= 0){
            _moveSpeed = moveSpeed;
        } else {
            dashTimer--;
        }
        if(dashCooldownTimer > 0)
        {
            dashCooldownTimer--;
        }
    }
    void Directional()
    {
        moveDir = inputDir * _moveSpeed; //inputDir is the normalized direction, moveDir includes moveSpeed
        rb.velocity = new Vector3(moveDir.x, rb.velocity.y, moveDir.y); //Assign velocity and keep current y-component of velocity

        //Smooth rotation
        if (inputDir.magnitude > 0.125f)
        {
            targetAngle = (Vector2.SignedAngle(inputDir, Vector2.up) + 360f) % 360f;
            smoothAngle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, targetAngle, ref smoothTurnVelocity, smoothTurnTime);
            transform.rotation = Quaternion.Euler(new Vector3(0, smoothAngle, 0));
        }
    }
    void OnMove(InputValue movementValue)
    {
        inputDir = movementValue.Get<Vector2>();
    }

    void OnDash(InputValue value)
    {
        if(dashCooldownTimer <= 0)
        {
            _moveSpeed = dashSpeed;
            dashTimer = dashTime;
            dashCooldownTimer = dashCooldown;
        }
    }

    void OnAttack(InputValue value)
    {
        
    }
}
