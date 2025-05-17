using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public string cs;   //aktualny stan do podgl¹du



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
    public float fallMultiplier = 2.5f;
    [Header("Dash")]
    public float dashSpeed = 100f;
    public float minDashDuration = 0.1f;
    public float maxDashDuration = 0.3f;
    public float dashCooldown = 2f;
    public float dashStoppingForce = 0.1f;
    [Header("Attack")]
    public float attackStateDuration = 0.4f;
    public float attackDashForce = 4f;         
    public float attackDashDuration = 0.15f;    
    public float attackHitboxRadius = 0.7f;    
    public float attackHitboxOffset = 0.5f;      
    public LayerMask enemyLayer;                 
    public float playerKnockbackOnHitForce = 6f;  
    public float playerKnockbackDuration = 0.1f;
    [Header("Jump Ascent")]
    public float jumpAscentAccelerationFactor = 2.0f;
    public float jumpAscentAccelerationDuration = 0.15f;
    public float jumpHoldEndForceMultiplier = 0.1f;
    [Header("Air Slash Cooldown")]
    public float airSlashBaseCooldown = 0.75f;
    public float airSlashPogoResetCooldown = 0.15f; 
    private float nextAirSlashReadyTime = 0f;
    public float currentPogoForceModifier = 1f;
    public float pogoForceReductionFactor = 0.75f;
    public float minPogoForceModifier = 0.2f;
    [Header("Ground Combo Settings")]
    public int currentComboStep = 0;
    [HideInInspector] public int maxComboSteps = 3;
    public float comboContinueWindow = 0.3f;
    public float lastAttackPhaseEndTime = 0f;
    public float attackInputBufferTime = 0.2f;
    private bool attackBuffered = false;
    private float lastAttackInputTime;
    public bool isAttacking = false;
    [Header("Block Settings")]
    public bool isInvulnerable = false;


    // Maszyna Stanów
    private StateMachine<PlayerController> stateMachine;

    // Referencja dla komponentów
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }

    // Referencje dla zmiennych gracza
    public float coyoteTime { get; private set; } = 0.2f;
    public float coyoteTimer { get; set; } = 0f;
    public float jumpBufferTime { get; private set; } = 0.15f;
    public float jumpBufferCounter { get; set; } = 0f;
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

        // Pobieranie mo¿liwych stanów gracza
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
        if (rb.linearVelocity.y < 0 && !PlayerInputHandler.Instance.dashHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }



        stateMachine?.OnUpdate();
        cs = stateMachine.currentState.name;



        HandleJumpBuffering();
        HandleAttackInputAndBuffering();
    }

    private void FixedUpdate()
    {
        stateMachine?.OnFixedUpdate();

        // Ograniczenie prêdkoœci w obu osiach
        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxSpeedX, maxSpeedX);
        float clampedY = Mathf.Clamp(rb.linearVelocity.y, -maxFallSpeed, maxRiseSpeed);
        rb.linearVelocity = new Vector2(clampedX, clampedY);
    }




    /// <summary>
    /// Sprawdza czy gracz jest na ziemi.
    /// </summary>
    /// <returns></returns>
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


    /// <summary>
    /// Sprawdza czy gracz znajduje siê na œcianie
    /// </summary>
    /// <param name="dir">Lewa czy Prawa œciana?</param>
    /// <returns></returns>
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

    /// <summary>
    /// Resetuje mo¿liwoœæ skoku gracza.
    /// </summary>
    public void ResetJumps()
    {
        jumpsRemaining = maxJumps;
        currentPogoForceModifier = 1f;
    }

    /// <summary>
    /// Sprawdza, czy atak w powietrzu jest gotowy do u¿ycia (czy min¹³ cooldown).
    /// </summary>
    public bool IsAirSlashReady()
    {
        return Time.time >= nextAirSlashReadyTime;
    }

    /// <summary>
    /// Aktywuje standardowy cooldown dla ataku w powietrzu.
    /// Wywo³ywane na pocz¹tku stanu PlayerAirSlashState.
    /// </summary>
    public void TriggerAirSlashCooldown()
    {
        nextAirSlashReadyTime = Time.time + airSlashBaseCooldown;
    }

    /// <summary>
    /// Aktywuje skrócony cooldown "pogo" po trafieniu przeciwnika.
    /// Pozwala na szybsze ponowne u¿ycie ataku.
    /// </summary>
    public void TriggerAirSlashPogoReset()
    {
        nextAirSlashReadyTime = Time.time + airSlashPogoResetCooldown;
    }

    /// <summary>
    /// Funkcja Buffera dla ataku
    /// </summary>
    public void HandleAttackInputAndBuffering()
    {
        if (PlayerInputHandler.Instance.attackTrigger)
        {
            attackBuffered = true;
            lastAttackInputTime = Time.time;
        }

        if (attackBuffered && Time.time > lastAttackInputTime + attackInputBufferTime)
        {
            attackBuffered = false;
        }

        if (!isAttacking && currentComboStep > 0 && Time.time > lastAttackPhaseEndTime + comboContinueWindow)
        {
            ResetCombo();
        }
    }

    public bool TryConsumeAttackInputForCombo()
    {
        if (attackBuffered)
        {
            attackBuffered = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Wywo³ywane przez stany ataku, gdy ich wewnêtrzny timer dobiegnie koñca.
    /// </summary>
    public void NotifyAttackPhaseFinished()
    {
        isAttacking = false; // Gracz ju¿ nie jest aktywnie w "fazie uderzenia"
        lastAttackPhaseEndTime = Time.time; // Zapisz czas zakoñczenia, aby comboContinueWindow mog³o dzia³aæ
                                            // Debug.Log($"[PC] Attack Phase Finished at {Time.time}. Current Combo Step (before potential advance): {currentComboStep}");

        // Inkrementuj krok combo TUTAJ, bo faza bie¿¹cego ataku siê zakoñczy³a
        // Oznacza to, ¿e np. Attack1 zosta³ wykonany, wiêc currentComboStep staje siê 1 (gotowy do Attack2)
        if (currentComboStep < maxComboSteps) // Zabezpieczenie, aby nie przekroczyæ max
        {
            currentComboStep++;
            // Debug.Log($"[PC] Combo Advanced. New Step: {currentComboStep}");
        }
    }

    public void ResetCombo()
    {
        currentComboStep = 0;
        attackBuffered = false;
        isAttacking = false;
    }

    /// <summary>
    /// Funkcja Buffera dla skoku
    /// </summary>
    public void HandleJumpBuffering()
    {
        if (IsGrounded() || IsOnWall(Vector2.left) || IsOnWall(Vector2.right))
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;
    }


    /// <summary>
    /// Ustawia stan nieœmiertelnoœci gracza.
    /// </summary>
    public void SetInvulnerability(bool status)
    {
        isInvulnerable = status;
        // Debug.Log($"Player Invulnerability: {isInvulnerable}");
        // Tutaj mo¿esz dodaæ logikê zmiany warstwy kolizji, efektów wizualnych itp.
        // np. gameObject.layer = status ? LayerMask.NameToLayer("InvulnerablePlayer") : LayerMask.NameToLayer("Player");
    }

    // Dodaj metodê do obs³ugi otrzymywania obra¿eñ, która sprawdza flagê isInvulnerable
    public void TakeDamage(float amount)
    {
        if (isInvulnerable)
        {
            // Debug.Log("Player blocked damage!");
            // Mo¿na tu dodaæ efekty wizualne/dŸwiêkowe zablokowania
            return;
        }
        // Standardowa logika otrzymywania obra¿eñ
        // health -= amount;
        // Debug.Log($"Player took {amount} damage.");
    }
}
