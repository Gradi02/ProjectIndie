using UnityEngine;

public class PlayerDashState : StateBase<PlayerController>
{
    private const float MIN_DASH_TIME = 1.5f;

    public override void Enter()
    {
        base.Enter();

        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");

        owner.rb.AddForce(inputHandler.lookInput * owner.dashForce, ForceMode2D.Impulse);
        owner.dashTimer = owner.dashCooldown;
    }

    public override void Execute()
    {
        base.Execute();

        if (inputHandler.jumpPressed)
        {
            owner.jumpBufferCounter = owner.jumpBufferTime;
        }

        if (timeInThisState < MIN_DASH_TIME) return;

        // SprawdŸ warunki przejœcia
        if (!owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
        }
        else if (owner.rb.linearVelocity.magnitude < MIN_MOVEMENT_THRESHOLD)
        {
            stateMachine.ChangeState(typeof(PlayerIdleState));
        }
        else
        {
            stateMachine.ChangeState(typeof(PlayerWalkState));
        }
    }

    public override void FixedExecute()
    {
        base.FixedExecute();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
