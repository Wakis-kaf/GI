namespace UnitFramework.Runtime
{
    public interface ILogWriter
    {
        public SaveDirPath SaveDirPath { get; }
        public string LogWritePath { get; set; }
      
        
        public bool SaveLogOnlyCurrent { get; set; }
        public bool LogClearOld { get; set; }
        public int LogFileMaxCount { get; set; }
    }
}