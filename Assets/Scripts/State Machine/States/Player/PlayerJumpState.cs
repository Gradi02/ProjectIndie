using UnityEngine;

public class PlayerJumpState : StateBase<PlayerController>
{
    public override void Enter()
    {
        base.Enter();
        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");
        owner.rb.linearVelocity = new Vector2(owner.rb.linearVelocity.x, owner.jumpForce);
    }

    public override void Execute()
    {
        base.Execute();

        // Sprawdü warunki przejúcia
        if (owner.rb.linearVelocity.y < 0 && !owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
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

        owner.rb.linearVelocity = new Vector2(inputHandler.moveInput.x * owner.walkSpeed, owner.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
