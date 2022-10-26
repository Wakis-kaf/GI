namespace UnitFramework.Runtime
{
    public class WebRequestComponent : SingletonComponentUnit<WebRequestComponent>, IController
    {
        public override string ComponentUnitName
        {
            get => "WebRequestComponent";
        }

        public string ControllerName { get; }

        private DownloadMgr m_DownloadMgr => DownloadMgr.I;

        public void DownloadAsync(DownloadUnit info)
        {
            m_DownloadMgr.DownloadAsync(info);
        }

        //同步不会调用回调函数
        public bool DownloadSync(DownloadUnit info)
        {
            return m_DownloadMgr.DownloadSync(info);
        }

        public void DeleteDownload(DownloadUnit info)
        {
            m_DownloadMgr.DeleteDownload(info);
        }

        //清理所有下载
        public void ClearAllDownloads()
        {
            m_DownloadMgr.ClearAllDownloads();
        }

        public void ControllerUpdate()
        {
            m_DownloadMgr.Update();
        }
    }
}