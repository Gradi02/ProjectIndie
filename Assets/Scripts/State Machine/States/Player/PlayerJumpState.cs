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

        owner.rb.linearVelocity = new Vector2(owner.rb.linearVelocity.x, 0f);
        owner.rb.AddForce(Vector2.up * owner.minJumpForce, ForceMode2D.Impulse);

        jumpHoldTimer = 0f;
        jumpKeyReleasedDuringJumpLogic = false;
        timeInThisState = 0f;
        owner.coyoteTimer = 0f;
    }

    public override void Execute()
    {
        base.Execute();

        if (inputHandler.jumpReleased)
        {
            jumpKeyReleasedDuringJumpLogic = true;
        }

        if (owner.dashTimer <= 0f && inputHandler.dashPressed && inputHandler.lookInput != Vector2.zero)
        {
            stateMachine.ChangeState(typeof(PlayerDashState));
        }

        if (timeInThisState < TIME_TO_CHECK_CONDITIONS) return;

        // SprawdŸ warunki przejœcia
        if (owner.rb.linearVelocity.y < 0 && !owner.IsGrounded())
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

        float desiredHorizontalVelocity = inputHandler.moveInput.x * owner.walkSpeed;
        owner.rb.linearVelocity = new Vector2(desiredHorizontalVelocity, owner.rb.linearVelocity.y);

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
