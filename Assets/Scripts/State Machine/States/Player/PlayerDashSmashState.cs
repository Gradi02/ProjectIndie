using UnityEngine;

public class PlayerDashSmashState : StateBase<PlayerController>
{
    [SerializeField] private ParticleSystem dashSmashParticle;
    private float cooldown = 0;

    private const float DASH_SMASH_COOLDOWN = 0.5f;


    public override void Enter()
    {
        base.Enter();

        if (clip != null)
            owner.animator.Play(clip.name);
        else
            Debug.Log($"Animation from state {this} is null!");

        owner.rb.linearVelocity = Vector2.zero;
        cooldown = DASH_SMASH_COOLDOWN;

        owner.ResetJumps();
    }

    public override void Execute()
    {
        base.Execute();

        if(cooldown > 0)
            cooldown -= Time.deltaTime;

        if (inputHandler.jumpPressed)
        {
            owner.jumpBufferCounter = owner.jumpBufferTime;
        }
    
        if(cooldown > 0)
        {
            return;
        }

        if (!owner.IsGrounded())
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
        else
        {
            stateMachine.ChangeState(typeof(PlayerIdleState));
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
