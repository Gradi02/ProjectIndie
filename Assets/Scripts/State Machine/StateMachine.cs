using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class StateMachine<T> where T : MonoBehaviour
{
    public StateBase<T> currentState { get; private set; }
    private Dictionary<Type, StateBase<T>> availableStates;
    private T ownerContext;


    public StateMachine(T owner, IEnumerable<StateBase<T>> stateComponents)
    {
        ownerContext = owner;
        availableStates = new Dictionary<Type, StateBase<T>>();

        Type determinedStartingStateType = null;

        if (stateComponents == null || !stateComponents.Any())
        {
            Debug.LogError($"No state components provided to StateMachine for owner {owner.name}.", owner);
            return;
        }

        foreach (var state in stateComponents)
        {
            if (state == null) continue;

            Type stateType = state.GetType();
            if (!availableStates.ContainsKey(stateType))
            {
                state.Initialize(this, ownerContext);
                availableStates.Add(stateType, state);

                if (Attribute.IsDefined(stateType, typeof(StartingStateAttribute)))
                {
                    if (determinedStartingStateType != null)
                    {
                        Debug.LogWarning($"Multiple states on {owner.name} are marked with [StartingState]. Using the first one found: {determinedStartingStateType.Name}. Conflicting: {stateType.Name}", owner);
                    }
                    else
                    {
                        determinedStartingStateType = stateType;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Duplicate state component of type {stateType.Name} found for owner {owner.name}. Ignoring duplicate.", state);
            }
        }

        if (determinedStartingStateType == null && availableStates.Count > 0)
        {
            determinedStartingStateType = availableStates.Keys.First();
            Debug.LogWarning($"No starting state explicitly defined or fallback provided for {owner.name}. Defaulting to first found state: {determinedStartingStateType.Name}", owner);
        }

        if (determinedStartingStateType != null && availableStates.TryGetValue(determinedStartingStateType, out StateBase<T> startingState))
        {
            currentState = startingState;
            currentState.Enter();
        }
        else if (availableStates.Count > 0)
        {
            Debug.LogError($"Could not determine or find a starting state for {owner.name}. StateMachine will not function correctly.", owner);
        }
    }

    public void OnUpdate()
    {
        currentState?.Execute();
    }

    public void OnFixedUpdate()
    {
        currentState?.FixedExecute();
    }

    public void ChangeState(Type newStateType)
    {
        if (currentState != null && currentState.GetType() == newStateType)
        {
            return; // Ju¿ jesteœ w tym stanie
        }

        if (availableStates.TryGetValue(newStateType, out StateBase<T> newState))
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }
        else
        {
            Debug.LogError($"State type {newStateType.Name} not found! Cannot change state.");
        }
    }
}
