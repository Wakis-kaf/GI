namespace UnitFramework.Runtime
{
#if UNITY_EDITOR


    [AutoRegisterHelper]
    [LogStackTraceIgnore]
    public class UnityLogPrintHelper : ILogPrintHelper
    {
        public void PrintLog(Log.LogData logData)
        {
            switch (logData.logLevel)
            {
                case LogLevel.WARN:
                    UnityEngine.Debug.LogWarning(logData.GetMessage());
                    break;
                case LogLevel.FATAL:
                case LogLevel.ERROR:
                    UnityEngine.Debug.LogError(logData.GetMessage());
                    break;
                default:
                    UnityEngine.Debug.Log(logData.GetMessage());
                    break;
            }
        }

        public void Dispose()
        {
        }
    }
#endif
}