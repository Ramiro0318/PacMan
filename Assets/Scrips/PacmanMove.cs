using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePacmanMove : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float collisionCheckDistance = 0.1f;
    public LayerMask wallLayer = default;

    private Animator _animator;

    private Vector2 movementInput = Vector2.zero;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
        TryMove();

    }

     void LateUpdate()
    {
        _animator.SetInteger("DireccionX", (int) movementInput.x);
        _animator.SetInteger("DireccionY", (int) movementInput.y);
        _animator.SetBool("Muerto", false);
    }

    void HandleInput()
    {
        movementInput = Vector2.zero;

        
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            movementInput.y = 1;
        else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            movementInput.y = -1;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            movementInput.x = 1;
        else if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            movementInput.x = -1;

        if (movementInput.magnitude > 1)
            movementInput.Normalize();
    }

    void TryMove()
    {
        if (movementInput.magnitude > 0.1f)
        {
           
            Vector2 moveDirection = movementInput.normalized;
            float distance = moveSpeed * Time.deltaTime + collisionCheckDistance;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, distance, wallLayer);

            Debug.DrawRay(transform.position, moveDirection * distance, hit.collider ? Color.red : Color.green);

            if (hit.collider == null)
            {
                transform.Translate(new Vector3(movementInput.x, movementInput.y, 0) * moveSpeed * Time.deltaTime);
            }
        }
    }
}