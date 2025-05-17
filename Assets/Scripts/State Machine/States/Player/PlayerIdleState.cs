using UnityEngine;


[StartingState]
public class PlayerIdleState : StateBase<PlayerController>
{
    public override void Enter()
    {
        base.Enter();
        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");
        owner.rb.linearVelocity = Vector2.zero;

        if(owner.jumpBufferCounter > 0f && owner.IsGrounded())
        {
            owner.jumpBufferCounter = 0;
            stateMachine.ChangeState(typeof(PlayerJumpState));
            return;
        }

        owner.dashUsed = false;
        owner.ResetJumps();
    }

    public override void Execute()
    {
        base.Execute();

        if (inputHandler.moveInput.x < -0.01f)
        {
            owner.spriteRenderer.flipX = true;
        }
        else if (inputHandler.moveInput.x > 0.01f)
        {
            owner.spriteRenderer.flipX = false;
        }

        // Sprawdü warunki przejúcia
        if (owner.TryConsumeAttackInputForCombo() && owner.IsGrounded() && !owner.isAttacking)
        {
            if (owner.currentComboStep == 0)
            {
                stateMachine.ChangeState(typeof(PlayerGroundAttack1State));
            }
            else if (owner.currentComboStep == 1 && Time.time < owner.lastAttackPhaseEndTime + owner.comboContinueWindow)
            {
                stateMachine.ChangeState(typeof(PlayerGroundAttack2State));
            }
            else if (owner.currentComboStep == 2 && Time.time < owner.lastAttackPhaseEndTime + owner.comboContinueWindow)
            {
                stateMachine.ChangeState(typeof(PlayerGroundAttack3State));
            }
            else
            {
                owner.ResetCombo();
                stateMachine.ChangeState(typeof(PlayerGroundAttack1State));
            }
        }
        else if (inputHandler.blockTrigger && owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerBlockState));
        }
        else if (!owner.dashUsed && inputHandler.dashPressed && inputHandler.moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(typeof(PlayerDashState));
        }
        else if (!owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
        }
        else if (inputHandler.jumpPressed)
        {
            stateMachine.ChangeState(typeof(PlayerJumpState));
        }
        else if (Mathf.Abs(owner.rb.linearVelocity.x) > MIN_MOVEMENT_THRESHOLD)
        {
            stateMachine.ChangeState(typeof(PlayerWalkState));
        }
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        float targetVelocityX = inputHandler.moveInput.x * owner.walkSpeed;
        float newVelocityX = Mathf.MoveTowards(owner.rb.linearVelocity.x, targetVelocityX, owner.acceleration * Time.fixedDeltaTime);
        owner.rb.linearVelocity = new Vector2(newVelocityX, owner.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
