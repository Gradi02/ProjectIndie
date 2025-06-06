using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public string cs;   //aktualny stan do podgl�du



    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;

    [Header("BoxCast Parameters")]
    public Vector2 boxSize = new Vector2(0.5f, 0.1f);
    public Vector2 wallBoxSize = new Vector2(0.1f, 0.5f);
    public float castDistance = 0.05f;

    [Header("-=-=- Movement Stats -=-=-")]
    [Header("Walk")]
    public float walkSpeed = 5f;
    public float maxSpeedX = 10f;
    public float maxFallSpeed = 10f;
    [Header("Jump")]
    public float maxRiseSpeed = 7f;
    public float minJumpForce= 5f;
    public float maxJumpHoldTime = 1f;
    public float additionalJumpForce = 8f;
    public int maxJumps = 2;
    [Header("Dash")]
    public float dashSpeed = 100f;
    public float minDashDuration = 0.1f;
    public float maxDashDuration = 0.3f;
    public float dashCooldown = 2f;
    public float dashStoppingForce = 0.1f;



    // Maszyna Stan�w
    private StateMachine<PlayerController> stateMachine;

    // Referencja dla komponent�w
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }

    // Referencje dla zmiennych gracza
    public float coyoteTime { get; private set; } = 0.2f;
    public float coyoteTimer { get; set; } = 0f;
    public float jumpBufferTime { get; private set; } = 0.15f;
    public float jumpBufferCounter { get; set; } = 0f;
    public float dashTimer { get; set; } = 0f;
    public float originalGravityScale { get; set; }
    public Vector2 wallDirFlag { get; set; } = Vector2.zero;
    public float acceleration { get; private set; } = 30f;
    public int jumpsRemaining { get; set; } = 0;
    public bool dashUsed { get; set; } = false;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Pobieranie mo�liwych stan�w gracza
        StateBase<PlayerController>[] stateComponents = GetComponentsInChildren<StateBase<PlayerController>>();
        if (stateComponents.Length == 0)
        {
            Debug.LogError($"No StateBase<PlayerController> components found on {gameObject.name}. StateMachine cannot be initialized.", this);
            return;
        }

        // Inicjalizacja maszyny
        stateMachine = new StateMachine<PlayerController>(this, stateComponents.ToList());

        ResetJumps();
    }

    private void Update()
    {
        stateMachine?.OnUpdate();
        cs = stateMachine.currentState.name;

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

        // Ograniczenie pr�dko�ci w obu osiach
        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxSpeedX, maxSpeedX);
        float clampedY = Mathf.Clamp(rb.linearVelocity.y, -maxFallSpeed, maxRiseSpeed);
        rb.linearVelocity = new Vector2(clampedX, clampedY);
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
        Transform check = (dir == Vector2.right ? wallCheckRight : wallCheckLeft);

        RaycastHit2D hit = Physics2D.BoxCast(
           check.position,
           wallBoxSize,              
           0f,                  
           dir,                 
           castDistance,        
           groundLayer         
       );

        return hit.collider != null;
    }

    public void ResetJumps()
    {
        jumpsRemaining = maxJumps;
    }
}
