using UnityEngine;

public class PlayerAttackState : StateBase<PlayerController>
{


    public override void Enter()
    {
        base.Enter();

        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");

        owner.rb.linearVelocity = Vector2.zero;

        owner.dashUsed = false;
        owner.ResetJumps();
    }

    public override void Execute()
    {
        base.Execute();



        // SprawdŸ warunki przejœcia
        if (!owner.dashUsed && inputHandler.dashPressed && inputHandler.lookInput != Vector2.zero)
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
        else if (owner.rb.linearVelocity.magnitude > MIN_MOVEMENT_THRESHOLD)
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
