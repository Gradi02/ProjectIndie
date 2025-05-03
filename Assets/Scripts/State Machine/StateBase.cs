using UnityEngine;

public abstract class StateBase<T> where T : MonoBehaviour
{
    protected StateMachine<T> stateMachine;
    protected T owner; // W³aœciciel kontekstu

    protected const float DELAY_BETWEEN_CHANGE_STATE = 0.05f; 
    protected float timeToEnableExit = 0;

    public virtual void Initialize(StateMachine<T> machine, T ownerContext)
    {
        this.stateMachine = machine;
        this.owner = ownerContext;
    }

    public virtual void Enter() 
    {
        timeToEnableExit = Time.time + DELAY_BETWEEN_CHANGE_STATE;
    }

    public virtual void Execute() { }
    public virtual void FixedExecute() { }
    public virtual void Exit() { }


    public bool CanExitState()
    {
        return timeToEnableExit < Time.time;
    }
}
