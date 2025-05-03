using UnityEngine;
using System;
using System.Collections.Generic;

public class StateMachine<T> : MonoBehaviour where T : MonoBehaviour
{

    [SerializeField] private string currentStateNameDebug; // Do podgl¹du w Inspektorze
    public StateBase<T> currentState { get; private set; }
    private Dictionary<Type, StateBase<T>> availableStates;
    private T ownerContext; // Referencja do w³aœciciela


    // Wywo³ywana po stworzeniu instancji maszyny
    public void Initialize(T owner, Dictionary<Type, StateBase<T>> states, Type startingStateType)
    {
        ownerContext = owner;
        availableStates = states;

        if (availableStates.TryGetValue(startingStateType, out StateBase<T> startingState))
        {
            currentState = startingState;
            currentState.Enter();
            currentStateNameDebug = currentState.GetType().Name;
        }
        else
        {
            Debug.LogError($"Starting state type {startingStateType.Name} not found in available states!");
        }
    }

    private void Update()
    {
        currentState?.Execute();
    }

    private void FixedUpdate()
    {
        currentState?.FixedExecute();
    }

    public void ChangeState(Type newStateType)
    {
        if (currentState != null && currentState.GetType() == newStateType)
        {
            return; // Ju¿ jesteœ w tym stanie
        }

        if(currentState != null && !currentState.CanExitState())
        {
            return; // Delay przejscia miedzy stanami by zapobiec dziwnej pêtli
        }

        if (availableStates.TryGetValue(newStateType, out StateBase<T> newState))
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
            currentStateNameDebug = currentState.GetType().Name; // Aktualizacja nazwy dla debugowania
        }
        else
        {
            Debug.LogError($"State type {newStateType.Name} not found! Cannot change state.");
        }
    }


    // Metody udostêpniaj¹ce kontekst dla stanów
    public Rigidbody2D GetRigidbody() => ownerContext.GetComponent<Rigidbody2D>(); // Przyk³ad
    public Animator GetAnimator() => ownerContext.GetComponent<Animator>();     // Przyk³ad
    public T GetOwnerContext() => ownerContext;
}
