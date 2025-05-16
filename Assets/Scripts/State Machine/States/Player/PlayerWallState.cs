using UnityEngine;

public class PlayerWallState : StateBase<PlayerController>
{
    private Vector2 wallDirection;
    private float wallSlideSpeed = 0;

    private const float MIN_WALL_HOLD_INPUT_THRESHOLD = 0.1f;
    private const float WALL_START_SLIDE_SPEED = 2f;
    private const float WALL_SLIDE_SPEED_FORCE = 0.03f;
    private const float WALL_MAX_SLIDE_SPEED = 15f;

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

        owner.rb.linearVelocity = Vector2.zero;

        if (wallDirection.x > -0.01f)
        {
            owner.spriteRenderer.flipX = true;
        }
        else if (wallDirection.x < 0.01f)
        {
            owner.spriteRenderer.flipX = false;
        }

        wallSlideSpeed = WALL_START_SLIDE_SPEED;
        owner.dashUsed = false;
        owner.ResetJumps();
    }

    public override void Execute()
    {
        base.Execute();


        // 1. Sprawdzenie, czy gracz chce siê odczepiæ, id¹c w przeciwnym kierunku
        bool wantsToDetachByMovingAway = false;
        if (wallDirection == Vector2.left && inputHandler.moveInput.x > MIN_WALL_HOLD_INPUT_THRESHOLD)
        {
            wantsToDetachByMovingAway = true;
        }
        else if (wallDirection == Vector2.right && inputHandler.moveInput.x < -MIN_WALL_HOLD_INPUT_THRESHOLD)
        {
            wantsToDetachByMovingAway = true;
        }

        if (wantsToDetachByMovingAway)
        {
            stateMachine.ChangeState(typeof(PlayerFallState));
            return;
        }



        // 2. pozostale warunki
        if (inputHandler.attackTrigger)
        {
            stateMachine.ChangeState(typeof(PlayerAttackState));
        }
        else if (inputHandler.jumpPressed)
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

        if (wallSlideSpeed < WALL_MAX_SLIDE_SPEED)
            wallSlideSpeed += WALL_SLIDE_SPEED_FORCE;

        targetVerticalVelocity = -wallSlideSpeed;

        owner.rb.linearVelocity = new Vector2(0f, targetVerticalVelocity);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
