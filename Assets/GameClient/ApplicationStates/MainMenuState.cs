using CoreFramework.Logging;
using CoreFramework.StateMachine;

namespace GameClient.ApplicationStates
{
    /// <summary>
    /// The active state when the player is sitting in the Main Menu.
    /// </summary>
    public class MainMenuState : IState
    {
        private readonly StateMachine _stateMachine;

        public MainMenuState(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            GameLogger.Log("Entering MainMenuState: Scene is active. Awaiting player input.");
            // Later, we will use the EventBus to tell the UI system to fade the menu in.
        }

        public void Tick()
        {
            // Listen for high-level application events or transitions
        }

        public void Exit()
        {
            GameLogger.Log("Exiting MainMenuState.");
        }
    }
}