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
            owner.ResetCombo(); // ResetCombo ustawia te� isAttacking na false
        }

        owner.isAttacking = false; // Na pewno nie atakujemy w bloku

        // Ustaw nie�miertelno��
        owner.SetInvulnerability(true);

        // Odtw�rz animacj� bloku
        if (clip != null)
            owner.animator.Play(clip.name, 0, 0f);
        else
            Debug.LogWarning($"Animation clip for {this.GetType().Name} is null!");

        // Zatrzymaj gracza
        owner.rb.linearVelocity = Vector2.zero;
        // owner.rb.isKinematic = true; // Opcjonalnie, je�li chcesz ca�kowicie zablokowa� fizyk�

        // Ustaw kierunek sprite'a na podstawie inputu lub ostatniego kierunku
        // Je�li gracz si� rusza (input kierunkowy), obr�� go.
        // W przeciwnym razie, nie zmieniaj flipX, blokuj w kierunku, w kt�rym jest zwr�cony.
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

        // Pozosta� w stanie bloku, dop�ki timer nie minie.
        // Gracz jest unieruchomiony. Mo�na tu doda� logik� np. parowania, je�li blok jest trzymany.
        // Na razie, jest to blok czasowy.

        if (stateTimer <= 0f)
        {
            // Debug.Log("Block duration ended. Transitioning to Idle/Walk.");
            TransitionToIdleOrWalk();
        }
        // Mo�na doda� warunek wcze�niejszego wyj�cia, np. puszczenie przycisku bloku,
        // ale na razie zak�adamy blok na okre�lony czas.
        // if (!inputHandler.blockHeld) // Je�li masz taki input
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

        // Zdejmij nie�miertelno��
        owner.SetInvulnerability(false);
        // owner.rb.isKinematic = false; // Je�li u�ywa�e�

        // Upewnij si�, �e combo jest zresetowane, na wypadek gdyby Enter nie z�apa� wszystkiego
        // (chocia� ResetCombo w Enter powinno wystarczy�)
        // owner.ResetCombo();
    }
}
