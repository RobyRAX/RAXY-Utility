using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Utility
{
    public class CustomDebugLogger : MonoBehaviour
    {
        [Button]
        public void Log(string message)
        {
            CustomDebug.Log(message);
        }

        [Button]
        public void LogWarning(string message)
        {
            CustomDebug.LogWarning(message);
        }

        [Button]
        public void LogError(string message)
        {
            CustomDebug.LogError(message);
        }
    }
}
