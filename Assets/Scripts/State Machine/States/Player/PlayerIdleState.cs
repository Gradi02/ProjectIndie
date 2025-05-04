using UnityEngine;

public class PlayerIdleState : StateBase<PlayerController>
{
    public override void Enter()
    {
        base.Enter();
        //owner.animator.Play("IdleAnimation");
       // owner.rb.velocity = new Vector2(0, owner.rb.velocity.y);
    }

    public override void Execute()
    {
        base.Execute();
        // SprawdŸ warunki przejœcia
        /*if (owner.IsJumpPressed() && owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerJumpState));
        }
        else if (Mathf.Abs(owner.GetHorizontalInput()) > 0.1f)
        {
            stateMachine.ChangeState(typeof(PlayerMoveState));
        }*/
    }

    public override void Exit()
    {
        base.Exit();
    }
}
