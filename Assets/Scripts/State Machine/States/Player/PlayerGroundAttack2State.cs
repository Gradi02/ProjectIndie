using UnityEngine;

public class PlayerGroundAttack2State : StateBase<PlayerController>
{
    public float attackActiveDuration = 0.4f;
    private float stateTimer;

    public override void Enter()
    {
        base.Enter();
        owner.isAttacking = true;
        stateTimer = attackActiveDuration;
        // Debug.Log($"Enter GroundAttack2State. Duration: {attackActiveDuration}");


        if (clip != null) 
            owner.animator.Play(clip.name, 0, 0f);
        else 
            Debug.LogWarning($"Animation clip for {this.GetType().Name} is null!");

        owner.rb.linearVelocity = new Vector2(0, owner.rb.linearVelocity.y);
        if (Mathf.Abs(inputHandler.moveInput.x) > 0.01f)
            owner.spriteRenderer.flipX = inputHandler.moveInput.x < 0;
    }

    public override void Execute()
    {
        base.Execute();
        stateTimer -= Time.deltaTime;

        if (inputHandler.jumpPressed)
        {
            owner.jumpBufferCounter = owner.jumpBufferTime;
        }

        if (stateTimer <= 0f)
        {
            // Debug.Log("[Attack1] Attack Phase Timer Ended.");
            owner.NotifyAttackPhaseFinished(); // Informuje PC, ¿e faza siê skoñczy³a
                                               // PC ustawi isAttacking=false i zaktualizuje currentComboStep
            TransitionToIdleOrWalk();
        }
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        float targetVelocityX = inputHandler.moveInput.x * owner.walkSpeed * 0.2f;
        float newVelocityX = Mathf.MoveTowards(owner.rb.linearVelocity.x, targetVelocityX, owner.acceleration / 2 * Time.fixedDeltaTime);
        owner.rb.linearVelocity = new Vector2(newVelocityX, owner.rb.linearVelocity.y);
    }

    private void TransitionToIdleOrWalk()
    {
        if (!owner.IsGrounded()) stateMachine.ChangeState(typeof(PlayerFallState));
        else if (Mathf.Abs(inputHandler.moveInput.x) > MIN_MOVEMENT_THRESHOLD)
            stateMachine.ChangeState(typeof(PlayerWalkState));
        else stateMachine.ChangeState(typeof(PlayerIdleState));
    }

    public override void Exit()
    {
        base.Exit();
        // Jeœli opuszczamy stan zanim timer dobieg³ koñca (np. przez skok, oberwanie)
        // i nie przechodzimy do nastêpnego ataku w combo.
        if (stateTimer > 0f)
        {
            // Debug.Log("Attack1: Exiting prematurely, resetting combo and attack flag.");
            owner.ResetCombo(); // Resetuje te¿ isAttacking
        }
        // Jeœli timer dobieg³ koñca, NotifyAttackPhaseFinished ju¿ ustawi³o isAttacking na false.
    }
}
