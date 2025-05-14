using UnityEngine;

public class PlayerAttackState : StateBase<PlayerController>
{
    private Vector2 attackDirection;
    private float stateTimer;          // Czas trwania ca³ego stanu ataku
    private float dashMovementTimer;   // Czas trwania ruchu "dash" w ramach ataku
    private float knockbackMovementTimer; // Czas trwania ruchu odepchniêcia gracza

    private bool hasHitEnemyThisAttack; // Zapobiega wielokrotnemu odepchniêciu w jednym ataku

    // Opcjonalnie: referencja do konkretnej animacji ataku, jeœli nie jest ustawiana globalnie
    // public AnimationClip attackAnimationClip; 

    public override void Enter()
    {
        base.Enter();

        // Odtwórz animacjê ataku
        // Jeœli masz dedykowan¹ zmienn¹ na klip:
        // if (attackAnimationClip != null) owner.animator.Play(attackAnimationClip.name);
        // Jeœli nazwa klipu jest sta³a lub ustawiana w `clip` z StateBase:
        if (clip != null)
            owner.animator.Play(clip.name); // Upewnij siê, ¿e 'clip' jest przypisany w inspektorze do tego stanu
        else
            Debug.LogWarning($"Animation clip for {this.GetType().Name} is null!");

        // Ustal kierunek ataku
        if (inputHandler.moveInput.sqrMagnitude > 0.1f) // Jeœli gracz u¿ywa prawego analoga/myszy
        {
            attackDirection = inputHandler.moveInput.normalized;
        }
        else // W przeciwnym razie, atakuj w kierunku, w którym gracz jest zwrócony
        {
            attackDirection = owner.spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        // Obróæ sprite'a (tylko horyzontalnie dla prostoty, pe³ne 360 wymaga³oby obrotu transform)
        if (attackDirection.x < -0.01f)
        {
            owner.spriteRenderer.flipX = true;
        }
        else if (attackDirection.x > 0.01f)
        {
            owner.spriteRenderer.flipX = false;
        }

        // Zastosuj pocz¹tkowy "dash"
        owner.rb.linearVelocity = attackDirection * owner.attackDashForce;

        // Inicjalizuj timery
        stateTimer = owner.attackStateDuration;
        dashMovementTimer = owner.attackDashDuration;
        knockbackMovementTimer = 0f; // Aktywowany tylko po trafieniu

        hasHitEnemyThisAttack = false;

        // Wykonaj pierwsze sprawdzenie trafienia (mo¿e byæ pomocne dla szybkich ataków)
        CheckForHit();
    }

    public override void Execute()
    {
        base.Execute();

        stateTimer -= Time.deltaTime;
        if (dashMovementTimer > 0) dashMovementTimer -= Time.deltaTime;
        if (knockbackMovementTimer > 0) knockbackMovementTimer -= Time.deltaTime;


        // Jeœli trwa odepchniêcie gracza, nie pozwól na inny ruch
        if (knockbackMovementTimer > 0)
        {
            // Prêdkoœæ zosta³a ju¿ ustawiona w CheckForHit()
            // Mo¿na by tu dodaæ np. spowalnianie odepchniêcia, jeœli nie jest to sta³a prêdkoœæ
        }
        // Jeœli trwa dash ataku (i nie ma odepchniêcia)
        else if (dashMovementTimer > 0)
        {
            // Prêdkoœæ dasha zosta³a ustawiona w Enter(). Mo¿na j¹ tu modyfikowaæ, np. spowalniaæ.
            // Dla prostoty, zak³adamy, ¿e jest to sta³y impuls.
            // owner.rb.velocity = attackDirection * owner.attackDashForce; // Utrzymanie prêdkoœci dasha
        }
        // Jeœli dash siê skoñczy³, ale stan ataku trwa (np. na dokoñczenie animacji)
        // i nie ma odepchniêcia.
        else if (dashMovementTimer <= 0 && knockbackMovementTimer <= 0)
        {
            // Zredukuj prêdkoœæ gracza, chyba ¿e grawitacja ma dzia³aæ normalnie.
            // Dla prostoty, pozwalamy grawitacji dzia³aæ, jeœli gracz jest w powietrzu.
            // Jeœli gracz jest na ziemi, mo¿na go zatrzymaæ lub spowolniæ.
            if (owner.IsGrounded())
            {
                owner.rb.linearVelocity = new Vector2(Mathf.MoveTowards(owner.rb.linearVelocity.x, 0, owner.acceleration * Time.deltaTime), owner.rb.linearVelocity.y);
            }
        }

        // SprawdŸ trafienie, jeœli jeszcze nie trafiono w tym ataku i nie jesteœmy odpychani
        if (!hasHitEnemyThisAttack && knockbackMovementTimer <= 0f)
        {
            CheckForHit();
        }

        // Warunek zakoñczenia stanu ataku
        if (stateTimer <= 0f && knockbackMovementTimer <= 0f) // Zakoñcz tylko, gdy odepchniêcie te¿ siê skoñczy³o
        {
            TransitionToNextState();
        }
    }

    private void CheckForHit()
    {
        // Kalkulacja punktu, z którego wychodzi hitbox ataku
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
            // Tutaj logika zadawania obra¿eñ przeciwnikowi, np.:
            // enemy.GetComponent<EnemyHealth>()?.TakeDamage(damageAmount);

            if (!hasHitEnemyThisAttack) // Upewnij siê, ¿e odepchniêcie nast¹pi tylko raz
            {
                hasHitEnemyThisAttack = true;

                // Odepchnij gracza w przeciwnym kierunku do ataku
                Vector2 knockbackDir = -attackDirection;
                owner.rb.linearVelocity = knockbackDir * owner.playerKnockbackOnHitForce;
                knockbackMovementTimer = owner.playerKnockbackDuration; // Ustaw czas trwania ruchu odepchniêcia

                // Zresetuj timer dasha, bo odepchniêcie ma priorytet
                dashMovementTimer = 0f;

                // Mo¿na dodaæ efekty wizualne/dŸwiêkowe trafienia
                break; // Zwykle chcemy przetworzyæ tylko jedno trafienie na "pchniêcie"
            }
        }
    }

    private void TransitionToNextState()
    {
        if (owner.IsGrounded())
        {
            // Jeœli jest jakiœ ruch, przejdŸ do Walk, inaczej do Idle
            // Dla uproszczenia, zawsze do Idle po ataku na ziemi, chyba ¿e gracz od razu zacznie siê ruszaæ
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
        // Mo¿na tu zresetowaæ czêœæ prêdkoœci, jeœli atak nie powinien przenosiæ pêdu
        // Np. jeœli atak zakoñczy³ siê w powietrzu, ale nie chcemy, ¿eby gracz lecia³ dalej z si³¹ dasha
        if (knockbackMovementTimer <= 0f && dashMovementTimer <= 0f) // Jeœli nie jesteœmy w trakcie odepchniêcia lub dasha przy wyjœciu
        {
            // owner.rb.velocity = new Vector2(0, owner.rb.velocity.y); // Zachowaj tylko prêdkoœæ pionow¹
        }
        // Upewnij siê, ¿e jeœli gracz wyl¹duje pod koniec stanu ataku, grawitacja jest poprawnie aplikowana
        // lub przechodzi do stanu Grounded poprawnie. TransitionToNextState() powinno to obs³u¿yæ.
    }
}
