using CoreFramework.Logging;
using CoreFramework.StateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameClient.ApplicationStates
{
    /// <summary>
    /// Handles the asynchronous loading of the Main Menu scene and necessary background data.
    /// </summary>
    public class LoadMainMenuState : IState
    {
        private readonly StateMachine _stateMachine;
        private AsyncOperation _asyncLoad;

        public LoadMainMenuState(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            GameLogger.Log("Entering LoadMainMenuState: Starting Async Scene Load.");

            // Starts loading the scene in the background without freezing the game
            _asyncLoad = SceneManager.LoadSceneAsync("MainMenu");

            if (_asyncLoad == null)
            {
                GameLogger.LogError("Failed to load 'MainMenu' scene. Is it in the Build Settings?");
            }
        }

        public void Tick()
        {
            if (_asyncLoad == null) return;

            // In a real game, you would use _asyncLoad.progress here to update a UI loading bar.

            if (_asyncLoad.isDone)
            {
                // The scene has fully loaded. Hand control over to the actual Main Menu State.
                _stateMachine.ChangeState(new MainMenuState(_stateMachine));
            }
        }

        public void Exit()
        {
            GameLogger.Log("Exiting LoadMainMenuState: Handoff complete.");
        }
    }
}