namespace UnitFramework.Runtime
{
    public interface ILogWriteHelper : IHelperBase
    {
        void WriteLog(Log.LogData logData,ILogWriter writer); 
    }
}