using UnityEngine;

public class PlayerJumpState : StateBase<PlayerController>
{
    private const float TIME_TO_CHECK_CONDITIONS = 0.1f;
    private float jumpHoldTimer;
    private bool jumpKeyReleasedDuringJumpLogic;
    private float wallEnterTime = 0;
    private bool inWall = false;

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

        if ((owner.IsOnWall(Vector2.left) || owner.IsOnWall(Vector2.right)) && !inWall)
        {
            wallEnterTime = Time.time;
            inWall = true;
        }
        if(inWall && !owner.IsOnWall(Vector2.left) && !owner.IsOnWall(Vector2.right))
        {
            inWall = false;
        }


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

        if (!owner.dashUsed && inputHandler.dashPressed && inputHandler.moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(typeof(PlayerDashState));
        }
        else if(inputHandler.jumpPressed && owner.jumpsRemaining > 0)
        {
            stateMachine.ChangeState(typeof(PlayerExtraJumpState));
        }
        else if (inputHandler.attackTrigger)
        {
            if (!owner.IsGrounded() && owner.IsAirSlashReady())
            {
                stateMachine.ChangeState(typeof(PlayerAirSlashState));
                return;
            }
        }

        if (timeInThisState < TIME_TO_CHECK_CONDITIONS) return;

        // SprawdŸ warunki przejœcia
        if (((owner.IsOnWall(Vector2.left) && inputHandler.moveInput.x < 0f) || (owner.IsOnWall(Vector2.right) && inputHandler.moveInput.x > 0f)) && (Time.time - wallEnterTime) < 0.2f)
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
            float currentAppliedForce;
            float baseAdditionalForce = owner.additionalJumpForce;

            if (jumpHoldTimer < owner.jumpAscentAccelerationDuration)
            {
                currentAppliedForce = baseAdditionalForce * owner.jumpAscentAccelerationFactor;
            }
            else
            {
                float decelerationPhaseDuration = owner.maxJumpHoldTime - owner.jumpAscentAccelerationDuration;

                if (decelerationPhaseDuration <= 0.001f)
                {
                    currentAppliedForce = baseAdditionalForce * owner.jumpHoldEndForceMultiplier;
                }
                else
                {
                    float timeIntoDecelerationPhase = jumpHoldTimer - owner.jumpAscentAccelerationDuration;
                    float progressInDecelerationPhase = Mathf.Clamp01(timeIntoDecelerationPhase / decelerationPhaseDuration);

                    currentAppliedForce = Mathf.Lerp(baseAdditionalForce, baseAdditionalForce * owner.jumpHoldEndForceMultiplier, progressInDecelerationPhase);
                }
            }

            owner.rb.AddForce(Vector2.up * currentAppliedForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
            jumpHoldTimer += Time.fixedDeltaTime;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
