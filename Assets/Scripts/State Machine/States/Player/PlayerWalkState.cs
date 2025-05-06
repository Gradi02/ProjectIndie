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
        if(!owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
        }
        else if (owner.IsJumpPressed() && owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerJumpState));
        }
        else if (owner.GetPlayerVelocity() < 0.1f)
        {
            stateMachine.ChangeState(typeof(PlayerIdleState));
        }
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        float speed = owner.IsPlayerSprinting() ? owner.walkSpeed * owner.sprintMultiplier : owner.walkSpeed;
        owner.rb.linearVelocity = new Vector2(inputHandler.moveInput.x * speed, owner.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
