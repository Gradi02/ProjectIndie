using UnityEngine;

public class PlayerFallState : StateBase<PlayerController>
{
    public override void Enter()
    {
        base.Enter();
        owner.animator.Play(owner.animationsCodes[typeof(PlayerFallState)]);
        //owner.animator.Play("IdleAnimation");
        // owner.rb.velocity = new Vector2(0, owner.rb.velocity.y);
    }

    public override void Execute()
    {
        base.Execute();

        if (!CanExitState()) return;

        // Sprawdü warunki przejúcia
        if (owner.IsGrounded())
        {
            if (owner.GetPlayerVelocity() < 0.1f)
            {
                stateMachine.ChangeState(typeof(PlayerIdleState));
            }
            else
            {
                stateMachine.ChangeState(typeof(PlayerWalkState));
            }
        }
        else if (owner.IsJumpPressed() && owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerJumpState));
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
