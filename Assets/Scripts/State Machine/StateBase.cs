using System;
using UnityEngine;

public abstract class StateBase<T> : MonoBehaviour where T : MonoBehaviour
{
    protected PlayerInputHandler inputHandler => PlayerInputHandler.Instance;

    protected StateMachine<T> stateMachine;
    protected T owner; // W³aœciciel kontekstu

    protected const float DELAY_BETWEEN_CHANGE_STATE = 0.05f; 
    protected float timeToEnableExit = 0;

    [SerializeField] protected AnimationClip clip;

    public virtual void Initialize(StateMachine<T> machine, T ownerContext)
    {
        this.stateMachine = machine;
        this.owner = ownerContext;
    }

    public virtual void Enter() 
    {

    }

    public virtual void Execute() { }
    public virtual void FixedExecute() { }
    public virtual void Exit() { }
}
