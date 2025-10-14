namespace RAXY.Utility
{
    public static class CustomDebug
    {
        [System.Diagnostics.Conditional("ENABLE_LOG")]
        static public void Log(object message, UnityEngine.Object context = null)
        {
            if (context == null)
            {
                UnityEngine.Debug.Log(message);
            }
            else
            {
                UnityEngine.Debug.Log(message, context);
            }
        }

        [System.Diagnostics.Conditional("ENABLE_LOG")]
        static public void LogError(object message, UnityEngine.Object context = null)
        {
            if (context == null)
            {
                UnityEngine.Debug.LogError(message);
            }
            else
            {
                UnityEngine.Debug.LogError(message, context);
            }
        }

        [System.Diagnostics.Conditional("ENABLE_LOG")]
        static public void LogWarning(object message, UnityEngine.Object context = null)
        {
            if (context == null)
            {
                UnityEngine.Debug.LogWarning(message);
            }
            else
            {
                UnityEngine.Debug.LogWarning(message, context);
            }
        }
    }
}