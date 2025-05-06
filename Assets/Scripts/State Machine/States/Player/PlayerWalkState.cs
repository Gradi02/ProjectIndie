using UnityEngine;

public class PlayerWalkState : StateBase<PlayerController>
{
    private PlayerInputHandler inputHandler => PlayerInputHandler.Instance;

    public override void Enter()
    {
        base.Enter();
        owner.animator.Play(owner.animationsCodes[typeof(PlayerWalkState)]);
    }

    public override void Execute()
    {
        base.Execute();

        if (!CanExitState()) return;

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
