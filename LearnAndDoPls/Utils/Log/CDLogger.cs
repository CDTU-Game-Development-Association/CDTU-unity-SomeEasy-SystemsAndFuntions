using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CDTU.Utils
{
    public static class CDLogger
    {
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message, Object context = null)
        {
            Debug.Log(message, context);
        }

        public static void LogWarning(object message, Object context = null)
        {
            Debug.LogWarning(message, context);
        }

        public static void LogError(object message, Object context = null)
        {
            Debug.LogError(message, context);
        }
    }
}
