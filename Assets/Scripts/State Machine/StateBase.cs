using System;
using UnityEngine;

public abstract class StateBase<T> : MonoBehaviour where T : MonoBehaviour
{
    public const float MIN_MOVEMENT_THRESHOLD = 0.1f;
    protected PlayerInputHandler inputHandler => PlayerInputHandler.Instance;

    protected StateMachine<T> stateMachine;
    protected T owner; // W³aœciciel kontekstu

    protected float timeInThisState = 0f;

    [SerializeField] protected AnimationClip clip;

    public virtual void Initialize(StateMachine<T> machine, T ownerContext)
    {
        this.stateMachine = machine;
        this.owner = ownerContext;
    }

    public virtual void Enter() 
    {
        timeInThisState = 0;
    }

    public virtual void Execute() 
    {
        timeInThisState += Time.deltaTime;
    }
    public virtual void FixedExecute() 
    { }
    public virtual void Exit() { }
}
