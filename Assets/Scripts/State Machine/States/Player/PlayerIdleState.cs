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

        if(owner.jumpBufferCounter > 0f && owner.IsGrounded())
        {
            owner.jumpBufferCounter = 0;
            stateMachine.ChangeState(typeof(PlayerJumpState));
            return;
        }
    }

    public override void Execute()
    {
        base.Execute();

        if (inputHandler.moveInput.x < -0.01f)
        {
            owner.spriteRenderer.flipX = true;
        }
        else if (inputHandler.moveInput.x > 0.01f)
        {
            owner.spriteRenderer.flipX = false;
        }

        // Sprawdü warunki przejúcia
        if (owner.dashTimer <= 0f && inputHandler.dashPressed && inputHandler.lookInput != Vector2.zero)
        {
            stateMachine.ChangeState(typeof(PlayerDashState));
        }
        else if ((owner.IsOnWall(Vector2.left) && inputHandler.moveInput.x < 0f) || (owner.IsOnWall(Vector2.right) && inputHandler.moveInput.x > 0f))
        {
            stateMachine.ChangeState(typeof(PlayerWallState));
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

        float targetVelocityX = inputHandler.moveInput.x * owner.walkSpeed;
        float newVelocityX = Mathf.MoveTowards(owner.rb.linearVelocity.x, targetVelocityX, owner.acceleration * Time.fixedDeltaTime);
        owner.rb.linearVelocity = new Vector2(newVelocityX, owner.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
