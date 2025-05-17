using UnityEngine;

public class PlayerRestingState : StateBase<PlayerController>
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


        base.Execute();

        if (inputHandler.moveInput.x < -0.01f)
        {
            owner.spriteRenderer.flipX = true;
        }
        else if (inputHandler.moveInput.x > 0.01f)
        {
            owner.spriteRenderer.flipX = false;
        }

        // SprawdŸ warunki przejœcia
        if (inputHandler.attackTrigger)
        {

        }
        else if (!owner.dashUsed && inputHandler.dashPressed && inputHandler.moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(typeof(PlayerDashState));
        }
        else if (!owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
        }
        else if (inputHandler.jumpPressed)
        {
            stateMachine.ChangeState(typeof(PlayerJumpState));
        }
        else if (Mathf.Abs(owner.rb.linearVelocity.x) > MIN_MOVEMENT_THRESHOLD)
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
