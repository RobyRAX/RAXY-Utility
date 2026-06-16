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
            Debug.Log(message);
        }

        [Button]
        public void LogWarning()
        {
            Debug.LogWarning(message);
        }

        [Button]
        public void LogError()
        {
            Debug.LogError(message);
        }
    }
}
