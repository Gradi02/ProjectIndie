using UnityEngine;

public class PlayerAttackState : StateBase<PlayerController>
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

        // Odtw�rz animacj� ataku
        // Je�li masz dedykowan� zmienn� na klip:
        // if (attackAnimationClip != null) owner.animator.Play(attackAnimationClip.name);
        // Je�li nazwa klipu jest sta�a lub ustawiana w `clip` z StateBase:
        if (clip != null)
            owner.animator.Play(clip.name); // Upewnij si�, �e 'clip' jest przypisany w inspektorze do tego stanu
        else
            Debug.LogWarning($"Animation clip for {this.GetType().Name} is null!");

        // Ustal kierunek ataku
        if (inputHandler.moveInput.sqrMagnitude > 0.1f) // Je�li gracz u�ywa prawego analoga/myszy
        {
            attackDirection = inputHandler.moveInput.normalized;
        }
        else // W przeciwnym razie, atakuj w kierunku, w kt�rym gracz jest zwr�cony
        {
            attackDirection = owner.spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        // Obr�� sprite'a (tylko horyzontalnie dla prostoty, pe�ne 360 wymaga�oby obrotu transform)
        if (attackDirection.x < -0.01f)
        {
            owner.spriteRenderer.flipX = true;
        }
        else if (attackDirection.x > 0.01f)
        {
            owner.spriteRenderer.flipX = false;
        }

        // Zastosuj pocz�tkowy "dash"
        owner.rb.linearVelocity = attackDirection * owner.attackDashForce;

        // Inicjalizuj timery
        stateTimer = owner.attackStateDuration;
        dashMovementTimer = owner.attackDashDuration;
        knockbackMovementTimer = 0f; // Aktywowany tylko po trafieniu

        hasHitEnemyThisAttack = false;

        // Wykonaj pierwsze sprawdzenie trafienia (mo�e by� pomocne dla szybkich atak�w)
        CheckForHit();
    }

    public override void Execute()
    {
        base.Execute();

        stateTimer -= Time.deltaTime;
        if (dashMovementTimer > 0) dashMovementTimer -= Time.deltaTime;
        if (knockbackMovementTimer > 0) knockbackMovementTimer -= Time.deltaTime;


        // Je�li trwa odepchni�cie gracza, nie pozw�l na inny ruch
        if (knockbackMovementTimer > 0)
        {
            // Pr�dko�� zosta�a ju� ustawiona w CheckForHit()
            // Mo�na by tu doda� np. spowalnianie odepchni�cia, je�li nie jest to sta�a pr�dko��
        }
        // Je�li trwa dash ataku (i nie ma odepchni�cia)
        else if (dashMovementTimer > 0)
        {
            // Pr�dko�� dasha zosta�a ustawiona w Enter(). Mo�na j� tu modyfikowa�, np. spowalnia�.
            // Dla prostoty, zak�adamy, �e jest to sta�y impuls.
            // owner.rb.velocity = attackDirection * owner.attackDashForce; // Utrzymanie pr�dko�ci dasha
        }
        // Je�li dash si� sko�czy�, ale stan ataku trwa (np. na doko�czenie animacji)
        // i nie ma odepchni�cia.
        else if (dashMovementTimer <= 0 && knockbackMovementTimer <= 0)
        {
            // Zredukuj pr�dko�� gracza, chyba �e grawitacja ma dzia�a� normalnie.
            // Dla prostoty, pozwalamy grawitacji dzia�a�, je�li gracz jest w powietrzu.
            // Je�li gracz jest na ziemi, mo�na go zatrzyma� lub spowolni�.
            if (owner.IsGrounded())
            {
                owner.rb.linearVelocity = new Vector2(Mathf.MoveTowards(owner.rb.linearVelocity.x, 0, owner.acceleration * Time.deltaTime), owner.rb.linearVelocity.y);
            }
        }

        // Sprawd� trafienie, je�li jeszcze nie trafiono w tym ataku i nie jeste�my odpychani
        if (!hasHitEnemyThisAttack && knockbackMovementTimer <= 0f)
        {
            CheckForHit();
        }

        // Warunek zako�czenia stanu ataku
        if (stateTimer <= 0f && knockbackMovementTimer <= 0f) // Zako�cz tylko, gdy odepchni�cie te� si� sko�czy�o
        {
            TransitionToNextState();
        }
    }

    private void CheckForHit()
    {
        // Kalkulacja punktu, z kt�rego wychodzi hitbox ataku
        Vector2 attackOrigin = (Vector2)owner.transform.position + attackDirection * owner.attackHitboxOffset;

        // Wizualizacja hitboxa w edytorze (opcjonalnie)
#if UNITY_EDITOR
        Debug.DrawRay(attackOrigin - Vector2.up * owner.attackHitboxRadius, Vector2.up * 2 * owner.attackHitboxRadius, Color.red, 0.1f);
        Debug.DrawRay(attackOrigin - Vector2.right * owner.attackHitboxRadius, Vector2.right * 2 * owner.attackHitboxRadius, Color.red, 0.1f);
#endif

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackOrigin, owner.attackHitboxRadius, owner.enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Trafiono: " + enemy.name);
            // Tutaj logika zadawania obra�e� przeciwnikowi, np.:
            // enemy.GetComponent<EnemyHealth>()?.TakeDamage(damageAmount);

            if (!hasHitEnemyThisAttack) // Upewnij si�, �e odepchni�cie nast�pi tylko raz
            {
                hasHitEnemyThisAttack = true;

                // Odepchnij gracza w przeciwnym kierunku do ataku
                Vector2 knockbackDir = -attackDirection;
                owner.rb.linearVelocity = knockbackDir * owner.playerKnockbackOnHitForce;
                knockbackMovementTimer = owner.playerKnockbackDuration; // Ustaw czas trwania ruchu odepchni�cia

                // Zresetuj timer dasha, bo odepchni�cie ma priorytet
                dashMovementTimer = 0f;

                // Mo�na doda� efekty wizualne/d�wi�kowe trafienia
                break; // Zwykle chcemy przetworzy� tylko jedno trafienie na "pchni�cie"
            }
        }
    }

    private void TransitionToNextState()
    {
        if (owner.IsGrounded())
        {
            // Je�li jest jaki� ruch, przejd� do Walk, inaczej do Idle
            // Dla uproszczenia, zawsze do Idle po ataku na ziemi, chyba �e gracz od razu zacznie si� rusza�
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
        // Mo�na tu zresetowa� cz�� pr�dko�ci, je�li atak nie powinien przenosi� p�du
        // Np. je�li atak zako�czy� si� w powietrzu, ale nie chcemy, �eby gracz lecia� dalej z si�� dasha
        if (knockbackMovementTimer <= 0f && dashMovementTimer <= 0f) // Je�li nie jeste�my w trakcie odepchni�cia lub dasha przy wyj�ciu
        {
            // owner.rb.velocity = new Vector2(0, owner.rb.velocity.y); // Zachowaj tylko pr�dko�� pionow�
        }
        // Upewnij si�, �e je�li gracz wyl�duje pod koniec stanu ataku, grawitacja jest poprawnie aplikowana
        // lub przechodzi do stanu Grounded poprawnie. TransitionToNextState() powinno to obs�u�y�.
    }
}
