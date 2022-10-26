namespace UnitFramework.Runtime
{
    public sealed partial class ArchiveSystem
    {
        public enum SaveMode
        {
            Json, // Json  化保存
            Binary, // 二进制保存
            Xml, // XML 化保存
        }
        
       
    }
    public enum SaveDirPath
    {
        PersistencePath,
        DataPath,
        StreamingPath,
        TemporaryCachePath
    }
    
}