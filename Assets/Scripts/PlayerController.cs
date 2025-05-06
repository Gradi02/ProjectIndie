using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private PlayerInputHandler inputHandler => PlayerInputHandler.Instance;


    [Header("Movement Stats")]
    [SerializeField] public float walkSpeed { get; private set; } = 5f;
    [SerializeField] public float sprintMultiplier { get; private set; } = 1.5f;
    [SerializeField] public float jumpForce { get; private set; } = 5f;


    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    private float horizontal;
    private bool isTowardsRight = true;
    private bool isDashing, isJumping;

    // Maszyna Stanów
    private StateMachine<PlayerController> stateMachine;
    public Dictionary<Type, string> animationsCodes;

    // Referencja dla stanów
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }


    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine<PlayerController>();

        // Deklaracja mo¿liwych stanów gracza
        var states = new Dictionary<Type, StateBase<PlayerController>>
        {
            { typeof(PlayerIdleState), new PlayerIdleState() },
            { typeof(PlayerWalkState), new PlayerWalkState() },
            { typeof(PlayerJumpState), new PlayerJumpState() },
            { typeof(PlayerFallState), new PlayerFallState() }
        };

        // Inicjalizacja stanów
        foreach (var state in states.Values)
        {
            state.Initialize(stateMachine, this);
        }

        // Inicjalizacja maszyny
        stateMachine.Initialize(this, states, typeof(PlayerIdleState));
    }

    /*void Update()
    {
        if (isDashing) return;

        horizontal = inputHandler.moveInput.x;

        if (inputHandler.jumpTrigger && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
        }

        if (isJumping && IsGrounded() && rb.linearVelocity.y < 0.01f)
        {
            StartCoroutine(EndJump());
        }

        if (inputHandler.jumpTrigger && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.4f);
        }

        Flip();
    }*/

    /*    private void FixedUpdate()
        {
            if (isDashing) return;

            float walkSpeed = inputHandler.sprintSpeed > 0f ? speed * sprintMultiplier : speed;
            rb.linearVelocity = new Vector2(horizontal * walkSpeed, rb.linearVelocity.y);
        }*/

    private void Update()
    {
        Flip();
    }

    private void Flip()
    {
        if (isTowardsRight && horizontal < 0f || !isTowardsRight && horizontal > 0f)
        {
            isTowardsRight = !isTowardsRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }



    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    public bool IsJumpPressed()
    {
        return inputHandler.jumpTrigger;
    }

    public float GetPlayerVelocity()
    {
        return Mathf.Abs(rb.linearVelocity.magnitude);
    }

    public bool IsPlayerSprinting()
    {
        return inputHandler.sprintSpeed > 0f;
    }
}
