using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private PlayerInputHandler inputHandler => PlayerInputHandler.Instance;
    public string cs;

    [Header("Movement Stats")]
    [SerializeField] public float walkSpeed { get; private set; } = 5f;
    [SerializeField] public float sprintMultiplier { get; private set; } = 1.5f;
    [SerializeField] public float jumpForce { get; private set; } = 8f;


    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    private float horizontal;
    private bool isTowardsRight = true;
    private bool isDashing, isJumping;

    // Maszyna Stanów
    private StateMachine<PlayerController> stateMachine;

    // Referencja dla stanów
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }


    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Pobieranie mo¿liwych stanów gracza
        StateBase<PlayerController>[] stateComponents = GetComponentsInChildren<StateBase<PlayerController>>();
        if (stateComponents.Length == 0)
        {
            Debug.LogError($"No StateBase<PlayerController> components found on {gameObject.name}. StateMachine cannot be initialized.", this);
            return;
        }

        // Inicjalizacja maszyny
        stateMachine = new StateMachine<PlayerController>(this, stateComponents.ToList());
    }

    private void Update()
    {
        stateMachine?.OnUpdate();
        cs = stateMachine.currentState.name;
        Flip();
    }

    private void FixedUpdate()
    {
        stateMachine?.OnFixedUpdate();
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
}
