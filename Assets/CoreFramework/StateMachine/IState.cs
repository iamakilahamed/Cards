namespace CoreFramework.StateMachine
{
    /// <summary>
    /// The generic contract for any state in the game.
    /// </summary>
    public interface IState
    {
        /// <summary>Called exactly once when the state becomes active.</summary>
        void Enter();

        /// <summary>Called every frame while the state is active.</summary>
        void Tick();

        /// <summary>Called exactly once when the state is being swapped out.</summary>
        void Exit();
    }
}