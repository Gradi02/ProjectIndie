using UnityEngine;

public class PlayerAirSlashState : StateBase<PlayerController>
{
    private Vector2 attackDirection;
    private float stateTimer;          // Czas trwania ca�ego stanu ataku
    private float dashMovementTimer;   // Czas trwania ruchu "dash" w ramach ataku
    private float knockbackMovementTimer; // Czas trwania ruchu odepchni�cia gracza

    private bool hasHitEnemyThisAttack; // Zapobiega wielokrotnemu odepchni�ciu w jednym ataku

    // Opcjonalnie: referencja do konkretnej animacji ataku, je�li nie jest ustawiana globalnie
    // public AnimationClip attackAnimationClip; 

    public override void Enter()
    {
        base.Enter();

        // WA�NE: Sprawdzenie, czy atak mo�e by� wykonany (owner.IsAirSlashReady()),
        // powinno idealnie odbywa� si� w stanie, kt�ry PRZECHODZI DO PlayerAirSlashState,
        // ZANIM wywo�a stateMachine.ChangeState().
        // Je�li ten stan jest aktywowany, zak�adamy, �e warunek cooldownu zosta� ju� sprawdzony.

        // Aktywuj standardowy cooldown dla tego ataku
        owner.TriggerAirSlashCooldown();

        // Odtw�rz animacj� ataku
        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.LogWarning($"Animation clip for {this.GetType().Name} is null!");

        // Ustal kierunek ataku
        if (inputHandler.moveInput.sqrMagnitude > 0.1f)
        {
            attackDirection = inputHandler.moveInput.normalized;
        }
        else
        {
            attackDirection = owner.spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        // Obr�� sprite'a
        if (attackDirection.x < -0.01f)
        {
            owner.spriteRenderer.flipX = true;
        }
        else if (attackDirection.x > 0.01f)
        {
            owner.spriteRenderer.flipX = false;
        }

        // Zastosuj pocz�tkowy "dash"
        owner.rb.linearVelocity = attackDirection * owner.attackDashForce; // U�ywam linearVelocity zgodnie z oryginalnym kodem

        // Inicjalizuj timery
        stateTimer = owner.attackStateDuration;
        dashMovementTimer = owner.attackDashDuration;
        knockbackMovementTimer = 0f;

        hasHitEnemyThisAttack = false;

        CheckForHit();
    }

    public override void Execute()
    {
        base.Execute();

        stateTimer -= Time.deltaTime;
        if (dashMovementTimer > 0) dashMovementTimer -= Time.deltaTime;
        if (knockbackMovementTimer > 0) knockbackMovementTimer -= Time.deltaTime;

        if (knockbackMovementTimer > 0)
        {
            // Pr�dko�� odepchni�cia ustawiona w CheckForHit()
        }
        else if (dashMovementTimer > 0)
        {
            // Utrzymanie pr�dko�ci dasha, je�li potrzebne, lub pozwolenie na naturalne wygaszenie
            // owner.rb.linearVelocity = attackDirection * owner.attackDashForce; 
        }
        else if (dashMovementTimer <= 0 && knockbackMovementTimer <= 0)
        {
            if (owner.IsGrounded())
            {
                // U�ywam linearVelocity i acceleration zgodnie z oryginalnym kodem
                owner.rb.linearVelocity = new Vector2(Mathf.MoveTowards(owner.rb.linearVelocity.x, 0, owner.acceleration * Time.deltaTime), owner.rb.linearVelocity.y);
            }
            // W powietrzu, grawitacja powinna dzia�a� normalnie.
        }

        if (!hasHitEnemyThisAttack && knockbackMovementTimer <= 0f)
        {
            CheckForHit();
        }

        if (stateTimer <= 0f && knockbackMovementTimer <= 0f)
        {
            TransitionToNextState();
        }
    }

    private void CheckForHit()
    {
        Vector2 attackOrigin = (Vector2)owner.transform.position + attackDirection * owner.attackHitboxOffset;

#if UNITY_EDITOR
        Debug.DrawRay(attackOrigin - Vector2.up * owner.attackHitboxRadius, Vector2.up * 2 * owner.attackHitboxRadius, Color.red, 0.1f);
        Debug.DrawRay(attackOrigin - Vector2.right * owner.attackHitboxRadius, Vector2.right * 2 * owner.attackHitboxRadius, Color.red, 0.1f);
#endif

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackOrigin, owner.attackHitboxRadius, owner.enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Trafiono: " + enemy.name);
            // enemy.GetComponent<EnemyHealth>()?.TakeDamage(damageAmount);

            if (!hasHitEnemyThisAttack)
            {
                hasHitEnemyThisAttack = true;

                Vector2 knockbackDir = -attackDirection;
                float actualKnockbackForce = owner.playerKnockbackOnHitForce * owner.currentPogoForceModifier;
                owner.rb.linearVelocity = knockbackDir * actualKnockbackForce;
                owner.currentPogoForceModifier = Mathf.Max(owner.minPogoForceModifier, owner.currentPogoForceModifier * owner.pogoForceReductionFactor);
                knockbackMovementTimer = owner.playerKnockbackDuration;
                dashMovementTimer = 0f;

                // *** KLUCZOWA ZMIANA DLA POGO JUMP ***
                // Po trafieniu wroga, aktywuj kr�tszy cooldown, aby umo�liwi� "pogojump"
                owner.TriggerAirSlashPogoReset();

                break;
            }
        }
    }

    private void TransitionToNextState()
    {
        if (owner.IsGrounded())
        {
            if (Mathf.Abs(inputHandler.moveInput.x) > 0.01f)
            {
                stateMachine.ChangeState(typeof(PlayerWalkState));
            }
            else
            {
                stateMachine.ChangeState(typeof(PlayerIdleState));
            }
        }
        else
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
        }
    }

    public override void Exit()
    {
        base.Exit();
        // Dodatkowe czyszczenie przy wyj�ciu, je�li potrzebne
    }
}
