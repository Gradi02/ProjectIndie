using UnityEngine;

public class PlayerIdleState : StateBase<PlayerController>
{
    public override void Enter()
    {
        base.Enter();
        owner.animator.Play(owner.animationsCodes[typeof(PlayerIdleState)]);
        owner.rb.linearVelocity = Vector2.zero;
    }

    public override void Execute()
    {
        base.Execute();

        if (!CanExitState()) return;

        // SprawdŸ warunki przejœcia
        if (owner.IsJumpPressed() && owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerJumpState));
        }
        else if (owner.GetPlayerVelocity() > 0.1f)
        {
            if (owner.IsGrounded())
                stateMachine.ChangeState(typeof(PlayerWalkState));
            else
                stateMachine.ChangeState(typeof(PlayerFallState));
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
