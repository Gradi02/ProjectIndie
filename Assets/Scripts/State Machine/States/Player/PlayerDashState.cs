using UnityEngine;

public class PlayerDashState : StateBase<PlayerController>
{
    private Vector2 dashDirection;
    private float dashStartTime;
    private bool isDashMovementActive;

    public override void Enter()
    {
        base.Enter();

        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");

        dashDirection = inputHandler.lookInput.normalized;

        dashStartTime = Time.time;
        isDashMovementActive = true;
        owner.dashTimer = owner.dashCooldown; 

        owner.originalGravityScale = owner.rb.gravityScale;
        owner.rb.gravityScale = 0f;

        owner.rb.linearVelocity = dashDirection * owner.dashSpeed;

        if (dashDirection.x < -0.01f)
        {
            owner.spriteRenderer.flipX = true;
        }
        else if (dashDirection.x > 0.01f)
        {
            owner.spriteRenderer.flipX = false;
        }
    }

    public override void Execute()
    {
        base.Execute();

        // Buforowanie skoku podczas dasha
        if (inputHandler.jumpPressed)
        {
            owner.jumpBufferCounter = owner.jumpBufferTime;
        }

        if (!isDashMovementActive)
        {
            // Dash siê zakoñczy³, sprawdŸ warunki przejœcia
            if ((owner.IsOnWall(Vector2.left) || owner.IsOnWall(Vector2.right)))
            {
                stateMachine.ChangeState(typeof(PlayerWallState));
            }
            else if (!owner.IsGrounded())
            {
                stateMachine.ChangeState(typeof(PlayerFallState));
            }
            else if (inputHandler.moveInput.x != 0)
            {
                stateMachine.ChangeState(typeof(PlayerWalkState));
            }
            else if (owner.rb.linearVelocity.magnitude < MIN_MOVEMENT_THRESHOLD)
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

        if (isDashMovementActive)
        {
            float currentDashTimeElapsed = Time.time - dashStartTime;

            owner.rb.linearVelocity = dashDirection * owner.dashSpeed;

            bool dashInputHeld = inputHandler.dashHeld;

            if (currentDashTimeElapsed >= owner.maxDashDuration || (currentDashTimeElapsed >= owner.minDashDuration && !dashInputHeld))
            {
                isDashMovementActive = false;
                owner.rb.linearVelocity = dashDirection * owner.dashSpeed * owner.dashStoppingForce;
            }
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (owner.rb != null) 
        {
            owner.rb.gravityScale = owner.originalGravityScale;
        }
    }
}
