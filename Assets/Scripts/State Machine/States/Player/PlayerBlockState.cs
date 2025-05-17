using UnityEngine;

public class PlayerBlockState : StateBase<PlayerController>
{
    public float blockDuration = 0.5f;
    private float stateTimer;

    public override void Enter()
    {
        base.Enter();
        // Debug.Log("Enter PlayerBlockState");

        // Przerwij combo natychmiast
        if (owner.currentComboStep > 0 || owner.isAttacking)
        {
            // Debug.Log("Block state interrupting combo/attack.");
            owner.ResetCombo(); // ResetCombo ustawia te¿ isAttacking na false
        }

        owner.isAttacking = false; // Na pewno nie atakujemy w bloku

        // Ustaw nieœmiertelnoœæ
        owner.SetInvulnerability(true);

        // Odtwórz animacjê bloku
        if (clip != null)
            owner.animator.Play(clip.name, 0, 0f);
        else
            Debug.LogWarning($"Animation clip for {this.GetType().Name} is null!");

        // Zatrzymaj gracza
        owner.rb.linearVelocity = Vector2.zero;
        // owner.rb.isKinematic = true; // Opcjonalnie, jeœli chcesz ca³kowicie zablokowaæ fizykê

        // Ustaw kierunek sprite'a na podstawie inputu lub ostatniego kierunku
        // Jeœli gracz siê rusza (input kierunkowy), obróæ go.
        // W przeciwnym razie, nie zmieniaj flipX, blokuj w kierunku, w którym jest zwrócony.
        if (Mathf.Abs(inputHandler.moveInput.x) > 0.01f)
        {
            owner.spriteRenderer.flipX = inputHandler.moveInput.x < 0;
        }

        stateTimer = blockDuration;
    }

    public override void Execute()
    {
        base.Execute(); // Dla logiki StateBase (np. clip loop)

        stateTimer -= Time.deltaTime;

        // Pozostañ w stanie bloku, dopóki timer nie minie.
        // Gracz jest unieruchomiony. Mo¿na tu dodaæ logikê np. parowania, jeœli blok jest trzymany.
        // Na razie, jest to blok czasowy.

        if (stateTimer <= 0f)
        {
            // Debug.Log("Block duration ended. Transitioning to Idle/Walk.");
            TransitionToIdleOrWalk();
        }
        // Mo¿na dodaæ warunek wczeœniejszego wyjœcia, np. puszczenie przycisku bloku,
        // ale na razie zak³adamy blok na okreœlony czas.
        // if (!inputHandler.blockHeld) // Jeœli masz taki input
        // {
        //     TransitionToIdleOrWalk();
        // }
    }

    private void TransitionToIdleOrWalk()
    {
        if (!owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
        }
        else if (Mathf.Abs(inputHandler.moveInput.x) > MIN_MOVEMENT_THRESHOLD)
        {
            stateMachine.ChangeState(typeof(PlayerWalkState));
        }
        else
        {
            stateMachine.ChangeState(typeof(PlayerIdleState));
        }
    }

    public override void Exit()
    {
        base.Exit();
        // Debug.Log("Exit PlayerBlockState");

        // Zdejmij nieœmiertelnoœæ
        owner.SetInvulnerability(false);
        // owner.rb.isKinematic = false; // Jeœli u¿ywa³eœ

        // Upewnij siê, ¿e combo jest zresetowane, na wypadek gdyby Enter nie z³apa³ wszystkiego
        // (chocia¿ ResetCombo w Enter powinno wystarczyæ)
        // owner.ResetCombo();
    }
}
