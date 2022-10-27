using System;

namespace UnitFramework.Runtime
{
    public class AssetsDownloadMgr : SingletonComponentUnit<AssetsDownloadMgr>
    {
        public override string ComponentUnitName { get=>"AssetsDownloadMgr"; }

        /// <summary>
        ///  从远程服务器下下载 ab包文件
        /// </summary>
        /// <param name="getFileName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public byte[] DownloadSync(string abFileName)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 打上文件下载标记
        /// </summary>
        /// <param name="getFileName"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void AddDownloadSetFlag(string abFileName)
        {
           
            
        }

        /// <summary>
        /// 异步下载文件
        /// </summary>
        /// <param name="getFileName"></param>
        /// <param name="action"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void DownloadAsync(string abFileName, Action action)
        {
           
        }

        /// <summary>
        /// 检测该文件是否更新 
        /// </summary>
        /// <param name="getFileName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsNeedDownload(string abFileName)
        {
            return false;
        }
    }
}