using UnityEngine;

public class PlayerJumpState : StateBase<PlayerController>
{
    public override void Enter()
    {
        base.Enter();
        owner.animator.Play(owner.animationsCodes[typeof(PlayerJumpState)]);
        owner.rb.linearVelocity = new Vector2(owner.rb.linearVelocity.x, owner.jumpForce);
    }

    public override void Execute()
    {
        base.Execute();


        if (!CanExitState()) return;

        // Sprawdü warunki przejúcia
        if (owner.IsGrounded() && owner.rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
        }
        else if (owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerIdleState));
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
