using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Utility
{
    public class CustomDebugLogger : MonoBehaviour
    {
        public string message;

        [Button]
        public void Log()
        {
            CustomDebug.Log(message);
        }

        [Button]
        public void LogWarning()
        {
            CustomDebug.LogWarning(message);
        }

        [Button]
        public void LogError()
        {
            CustomDebug.LogError(message);
        }
    }
}
