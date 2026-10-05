using CoreFramework.Logging;
using CoreFramework.Pooling;
using CoreFramework.ServiceLocator;
using CoreFramework.StateMachine;
using UnityEngine;

namespace GameClient.Boot
{
    /// <summary>
    /// The singular entry point for the entire game. 
    /// Sits in the empty "Boot" scene and initializes all systems deterministically.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameBootstrapper : MonoBehaviour
    {
        // The master state machine that will control the client's flow
        private StateMachine _clientStateMachine;

        private void Awake()
        {
            // Keep the bootstapper alive forever
            DontDestroyOnLoad(this.gameObject);

            GameLogger.Log("=== Starting Game Boot Sequence ===");

            // Global Configuration
            SetupApplicationSettings();

            // System Registration
            RegisterCoreServices();

            // The Handoff
            InitializeMasterStateMachine();
        }

        private void Update()
        {
            // The bootstrapper's only job after Awake is to keep the state machine ticking
            _clientStateMachine?.Tick();
        }

        private void SetupApplicationSettings()
        {
            // Lock framerate to prevent mobile devices from melting and PC fans from spinning up
            Application.targetFrameRate = 60;

            // Keeps the network connection alive if the player tabs out of the game
            Application.runInBackground = true;

            GameLogger.Log("Application settings configured.");
        }

        private void RegisterCoreServices()
        {
            GameLogger.Log("Registering Core Service...");

            // Instantiate and lock in the object pool
            PoolService poolService = new PoolService();
            ServiceLocator.Register<IPoolService>(poolService);

            // Note: In the future, we will register the AudioService, UserProfileService,
            // and NetworkClient here.
        }

        private void InitializeMasterStateMachine()
        {
            GameLogger.Log("Initializing Client State Machine...");

            _clientStateMachine = new StateMachine();

            // Kick off the application flow!
            _clientStateMachine.ChangeState(new GameClient.ApplicationStates.LoadMainMenuState(_clientStateMachine));

            GameLogger.Log("Boot sequence complete. Awaiting handoff to Main Menu.");
        }
    }
}