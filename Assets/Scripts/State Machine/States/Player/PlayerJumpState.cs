using UnityEngine;

public class PlayerJumpState : StateBase<PlayerController>
{
    private const float TIME_TO_CHECK_CONDITIONS = 0.1f;
    private float jumpHoldTimer;
    private bool jumpKeyReleasedDuringJumpLogic;

    public override void Enter()
    {
        base.Enter();
        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");

        if (owner.wallDirFlag != Vector2.zero)
        {
            owner.rb.linearVelocity = Vector2.zero;
            Vector2 jumpDir = (Vector2.up * owner.minJumpForce) + (-owner.wallDirFlag * owner.minJumpForce);
            owner.rb.AddForce(jumpDir, ForceMode2D.Impulse);
        }
        else
        {
            owner.rb.linearVelocity = new Vector2(owner.rb.linearVelocity.x, 0f);
            owner.rb.AddForce(Vector2.up * owner.minJumpForce, ForceMode2D.Impulse);
        }

        jumpHoldTimer = 0f;
        jumpKeyReleasedDuringJumpLogic = false;
        timeInThisState = 0f;
        owner.coyoteTimer = 0f;
        owner.wallDirFlag = Vector2.zero;
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


        if (inputHandler.jumpReleased)
        {
            jumpKeyReleasedDuringJumpLogic = true;
        }

        if (owner.dashTimer <= 0f && inputHandler.dashPressed && inputHandler.lookInput != Vector2.zero)
        {
            stateMachine.ChangeState(typeof(PlayerDashState));
        }
        else if(inputHandler.jumpPressed && owner.jumpsRemaining > 0 && !owner.dashUsed)
        {
            stateMachine.ChangeState(typeof(PlayerExtraJumpState));
        }

        if (timeInThisState < TIME_TO_CHECK_CONDITIONS) return;

        // SprawdŸ warunki przejœcia
        if ((owner.IsOnWall(Vector2.left) && inputHandler.moveInput.x < 0f) || (owner.IsOnWall(Vector2.right) && inputHandler.moveInput.x > 0f))
        {
            stateMachine.ChangeState(typeof(PlayerWallState));
        }
        else if (owner.rb.linearVelocity.y < 0 && !owner.IsGrounded())
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
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

        float targetVelocityX = inputHandler.moveInput.x * owner.walkSpeed;
        float newVelocityX = Mathf.MoveTowards(owner.rb.linearVelocity.x, targetVelocityX, owner.acceleration/2 * Time.fixedDeltaTime);
        owner.rb.linearVelocity = new Vector2(newVelocityX, owner.rb.linearVelocity.y);

        if (inputHandler.jumpHeld && !jumpKeyReleasedDuringJumpLogic && jumpHoldTimer < owner.maxJumpHoldTime)
        {
            owner.rb.AddForce(Vector2.up * owner.additionalJumpForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
            jumpHoldTimer += Time.fixedDeltaTime;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
