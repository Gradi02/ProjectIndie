using UnityEngine;

public class PlayerFallState : StateBase<PlayerController>
{
    public override void Enter()
    {
        base.Enter();
        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");
    }

    public override void Execute()
    {
        base.Execute();

        if (inputHandler.jumpPressed)
        {
            owner.jumpBufferCounter = owner.jumpBufferTime;
        }

        // Sprawdü warunki przejúcia
        if (owner.dashTimer <= 0f && inputHandler.dashPressed && inputHandler.lookInput != Vector2.zero)
        {
            stateMachine.ChangeState(typeof(PlayerDashState));
        }
        else if (inputHandler.jumpPressed && owner.coyoteTimer > 0f)
        {
            stateMachine.ChangeState(typeof(PlayerJumpState));
        }
        else if (owner.IsGrounded())
        {
            if (owner.rb.linearVelocity.magnitude < MIN_MOVEMENT_THRESHOLD)
            {
                stateMachine.ChangeState(typeof(PlayerIdleState));
            }
            else
            {
                stateMachine.ChangeState(typeof(PlayerWalkState));
            }
        }
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        owner.rb.linearVelocity = new Vector2(inputHandler.moveInput.x * owner.walkSpeed * 0.8f, owner.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
