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
