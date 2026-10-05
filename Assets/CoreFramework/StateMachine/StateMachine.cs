using UnityEngine;

namespace CoreFramework.StateMachine
{
    /// <summary>
    /// A generic state machine that manages IState transitions and execution.
    /// It has zero knowledge of the actual game logic.
    /// </summary>
    public class StateMachine
    {
        private IState _currentState;

        /// <summary>
        /// Safely exits the current state and enters the new one.
        /// </summary>
        public void ChangeState(IState newState)
        {
            if (newState == null)
            {
                Debug.LogError("[StateMachine] Cannot transition to a null state.");
                return;
            }

            // Prevent swapping to the exact same state instance if we are already in it.
            if (_currentState == newState)
            {
                return;
            }

            // 1. Exit the old state (if it exists)
            _currentState?.Exit();

            // 2. Swap to the new state
            _currentState = newState;

            // 3. Enter the new state
            _currentState.Enter();
        }

        /// <summary>
        /// Drives the current state. This must be called from a MonoBehaviour's Update loop 
        /// somewhere in your game client or server.
        /// </summary>
        public void Tick()
        {
            _currentState?.Tick();
        }

        /// <summary>
        /// Useful for debugging or checking active states without exposing the private variable.
        /// </summary>
        public IState GetCurrentState()
        {
            return _currentState;
        }
    }
}