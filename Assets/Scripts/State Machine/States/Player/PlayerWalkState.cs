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
    }

    public override void Execute()
    {
        base.Execute();

        // Sprawdü warunki przejúcia
        if (!owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
        }
        else if (inputHandler.jumpTrigger)
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
