using UnityEngine;

public class PlayerWalkState : StateBase<PlayerController>
{
    public override void Enter()
    {
        base.Enter();

        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");

        if (owner.jumpBufferCounter > 0f && owner.IsGrounded())
        {
            owner.jumpBufferCounter = 0;
            stateMachine.ChangeState(typeof(PlayerJumpState));
            return;
        }
    }

    public override void Execute()
    {
        base.Execute();

        // Sprawdü warunki przejúcia
        if(owner.dashTimer <= 0f && inputHandler.dashPressed && inputHandler.lookInput != Vector2.zero)
        {
            stateMachine.ChangeState(typeof(PlayerDashState));
        }
        else if (!owner.IsGrounded() && ((owner.IsOnWall(Vector2.left) && inputHandler.moveInput.x < -0.1f) || (owner.IsOnWall(Vector2.right) && inputHandler.moveInput.x > 0.1f)))
        {
            stateMachine.ChangeState(typeof(PlayerWallState));
        }
        else if (!owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
        }
        else if (inputHandler.jumpPressed)
        {
            stateMachine.ChangeState(typeof(PlayerJumpState));
        }
        else if (owner.rb.linearVelocity.magnitude < MIN_MOVEMENT_THRESHOLD)                                                                             
        {
            stateMachine.ChangeState(typeof(PlayerIdleState));
        }
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        owner.rb.linearVelocity = new Vector2(inputHandler.moveInput.x * owner.walkSpeed, owner.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
