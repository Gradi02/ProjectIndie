using UnityEngine;

public class PlayerAttackState : StateBase<PlayerController>
{
    private Vector2 wallDirection;
    private float wallHoldTimer;

    private const float MIN_WALL_HOLD_INPUT_THRESHOLD = 0.1f;
    private const float WALL_HOLD_DURATION = 0.7f;
    private const float WALL_SLIDE_SPEED = 1.5f;

    public override void Enter()
    {
        base.Enter();

        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");

        if (owner.IsOnWall(Vector2.left))
        {
            wallDirection = Vector2.left;
        }
        else if (owner.IsOnWall(Vector2.right))
        {
            wallDirection = Vector2.right;
        }
        else
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
            return;
        }

        wallHoldTimer = WALL_HOLD_DURATION;
        owner.rb.linearVelocity = Vector2.zero;

        if (wallDirection.x > -0.01f)
        {
            owner.spriteRenderer.flipX = true;
        }
        else if (wallDirection.x < 0.01f)
        {
            owner.spriteRenderer.flipX = false;
        }

        owner.dashUsed = false;
        owner.ResetJumps();
    }

    public override void Execute()
    {
        base.Execute();


        if (inputHandler.jumpPressed)
        {
            owner.wallDirFlag = wallDirection;
            stateMachine.ChangeState(typeof(PlayerJumpState));
        }
        else if (owner.IsGrounded())
        {
            if (Mathf.Abs(inputHandler.moveInput.x) < MIN_WALL_HOLD_INPUT_THRESHOLD && owner.rb.linearVelocity.magnitude < MIN_MOVEMENT_THRESHOLD)
            {
                stateMachine.ChangeState(typeof(PlayerIdleState));
            }
            else
            {
                stateMachine.ChangeState(typeof(PlayerWalkState));
            }
        }
        else if (!owner.IsOnWall(wallDirection))
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
        }
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        float targetVerticalVelocity;

        if (wallHoldTimer > 0f)
        {
            targetVerticalVelocity = 0f;
        }
        else
        {
            targetVerticalVelocity = -WALL_SLIDE_SPEED;
        }

        owner.rb.linearVelocity = new Vector2(0f, targetVerticalVelocity);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
