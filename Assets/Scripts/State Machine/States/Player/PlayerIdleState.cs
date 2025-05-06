using UnityEngine;


[StartingState]
public class PlayerIdleState : StateBase<PlayerController>
{
    public override void Enter()
    {
        base.Enter();
        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");
        owner.rb.linearVelocity = Vector2.zero;
    }

    public override void Execute()
    {
        base.Execute();



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
