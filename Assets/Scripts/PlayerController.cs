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
    public float walkSpeed { get; private set; } = 5f;
    public float minJumpForce { get; private set; } = 5f;
    public float maxJumpHoldTime { get; private set; } = 1f;
    public float additionalJumpForce { get; private set; } = 8f;
    public float coyoteTime { get; private set; } = 0.2f;
    public float coyoteTimer { get; set; } = 0f;
    public float jumpBufferTime { get; private set; } = 0.15f;
    public float jumpBufferCounter { get; set; } = 0f;

    public float dashSpeed { get; private set; } = 25f;
    public float minDashDuration { get; private set; } = 0.1f;    
    public float maxDashDuration { get; private set; } = 0.8f;   
    public float dashCooldown { get; private set; } = 2f;       
    public float dashStoppingForce { get; private set; } = 0.1f;
    public float dashTimer { get; set; } = 0f;
    public float originalGravityScale { get; set; }


    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("BoxCast Parameters")]
    public Vector2 boxSize = new Vector2(0.5f, 0.1f);
    public float castDistance = 0.05f;

    // Maszyna Stanów
    private StateMachine<PlayerController> stateMachine;

    // Referencja dla stanów
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }

    // Others
    private bool isTowardsRight = false;
    public float moveSpeed = 10f;
    public float maxSpeedX = 5f;
    public float maxFallSpeed = 10f;
    public float maxRiseSpeed = 7f;


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

        if (IsGrounded())
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;
        
        if(dashTimer > 0f)
            dashTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        stateMachine?.OnFixedUpdate();

        // Ograniczenie prêdkoœci w obu osiach
        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxSpeedX, maxSpeedX);
        float clampedY = Mathf.Clamp(rb.linearVelocity.y, -maxFallSpeed, maxRiseSpeed);
        rb.linearVelocity = new Vector2(clampedX, clampedY);
    }

    private void Flip()
    {
        if (isTowardsRight && rb.linearVelocity.x < 0f || !isTowardsRight && rb.linearVelocity.x > 0f)
        {
            isTowardsRight = !isTowardsRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }


    public bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
           groundCheck.position,
           boxSize,             
           0f,                
           Vector2.down,        
           castDistance,        
           groundLayer           
       );

        return hit.collider != null;
    }

    public bool IsOnWall(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.BoxCast(
           groundCheck.position,
           boxSize,              
           0f,                  
           dir,                 
           castDistance,        
           groundLayer         
       );

        return hit.collider != null;
    }
}
