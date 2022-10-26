namespace UnitFramework.Runtime.Archives
{
    public interface IArchiveWriter<T> 
    {
        /// <summary>
        /// 当存档被读取的时候调用
        /// </summary>
        /// <param name="archive"></param>
        void OnArchiveLoad(Archive<T> archive );

        /// <summary>
        /// 当存档保存的时候调用
        /// </summary>
        /// <param name="archive"></param>
        void OnArchieSave(Archive<T> archive);
    }
}