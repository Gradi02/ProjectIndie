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

        if (inputHandler.moveInput.x < -0.01f)
        {
            owner.spriteRenderer.flipX = true;
        }
        else if (inputHandler.moveInput.x > 0.01f)
        {
            owner.spriteRenderer.flipX = false;
        }

        if (inputHandler.jumpPressed)
        {
            owner.jumpBufferCounter = owner.jumpBufferTime;
        }

        // Sprawdü warunki przejúcia
        if (inputHandler.attackTrigger)
        {
            stateMachine.ChangeState(typeof(PlayerAttackState));
        }
        else if (!owner.dashUsed && inputHandler.dashPressed && inputHandler.lookInput != Vector2.zero)
        {
            stateMachine.ChangeState(typeof(PlayerDashState));
        }
        else if ((owner.IsOnWall(Vector2.left) && inputHandler.moveInput.x < 0f) || (owner.IsOnWall(Vector2.right) && inputHandler.moveInput.x > 0f))
        {
            stateMachine.ChangeState(typeof(PlayerWallState));
        }
        else if (inputHandler.jumpPressed)
        {
            if(owner.jumpsRemaining == owner.maxJumps && owner.coyoteTimer > 0f)
                stateMachine.ChangeState(typeof(PlayerJumpState));
            else if(owner.jumpsRemaining > 0 && !owner.dashUsed)
                stateMachine.ChangeState(typeof(PlayerExtraJumpState));
        }
        else if (owner.IsGrounded())
        {
            if (owner.rb.linearVelocity.magnitude < MIN_MOVEMENT_THRESHOLD)
            {
                stateMachine.ChangeState(typeof(PlayerIdleState));
            }
            else
            {
                stateMachine.ChangeState(typeof(PlayerWalkState));
            }
        }
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        float targetVelocityX = inputHandler.moveInput.x * owner.walkSpeed * 0.8f;
        float newVelocityX = Mathf.MoveTowards(owner.rb.linearVelocity.x, targetVelocityX, owner.acceleration/2 * Time.fixedDeltaTime);
        owner.rb.linearVelocity = new Vector2(newVelocityX, owner.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
