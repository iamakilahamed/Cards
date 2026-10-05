using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CoreFramework.Logging
{
    /// <summary>
    /// A high-performance custom logger. 
    /// Methods are automatically stripped from the final build unless ENABLE_LOGS is defined,
    /// ensuring zero CPU or memory cost in the production release.
    /// </summary>
    public static class GameLogger
    {
        // The compiler symbol that dictates whether logs exist in the build.
        private const string LogDefine = "ENABLE_LOGS";

        /// <summary>
        /// Standard log, completely removed from builds unless ENABLE_LOGS is active.
        /// Always runs in the Unity Editor.
        /// </summary>
        [Conditional(LogDefine)]
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message)
        {
            Debug.Log(message);
        }

        [Conditional(LogDefine)]
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        [Conditional(LogDefine)]
        [Conditional("UNITY_EDITOR")]
        public static void LogError(string message)
        {
            // Errors usually shouldn't be stripped even in production, but in a strict 
            // AAA environment, we often handle production errors via crash-reporting tools 
            // (like Crashlytics) instead of Unity's Debug.LogError.
            Debug.LogError(message);
        }
    }
}