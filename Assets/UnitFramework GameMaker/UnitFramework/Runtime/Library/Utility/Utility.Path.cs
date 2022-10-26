using System.IO;
using UnityEngine;

namespace UnitFramework.Utils
{
    public static partial class Utility
    {
        /// <summary>
        /// unity 中常见的资源路径如下：
        /// 1. Application.dataPath 此属性用于返回项目文件所在文件夹的路径。例如在editor 中就是Asset
        /// 2. Application.streamingAssetsPath 此属性用于返回流数据的缓存目录，返回路径为相对路径，适合设置一些外部数据文件的路径
        /// 3. Application.persistenDataPath 此属性用于返回一个持久化数据存储目录的路径，可以在此路径下存放一些持久化的数据文件
        /// 4. Application.temporayCachePath 此属性用于返回一个临时数据的缓存目录
        /// 相关链接: https://zhuanlan.zhihu.com/p/125109062#:
        /// ~:text=%E4%BA%8C%E3%80%81Unity3D%E4%B8%AD%E7%9A%84%E8%B5%84%E6%BA%90%E8%AE%BF%E9%97%AE%E4%BB%8B%E7%BB%8D%201%E3%80%81Resources%20%E6%98%AFUnity3D%E7%B3%BB%E7%BB%9F%E6%8C%87%E5%AE%9A%E6%96%87%E4%BB%B6%E5%A4%B9%EF%BC%8C%E5%A6%82%E6%9E%9C%E4%BD%A0%E6%96%B0%E5%BB%BA%E7%9A%84%E6%96%87%E4%BB%B6%E5%A4%B9%E7%9A%84%E5%90%8D%E5%AD%97%E5%8F%ABResources%EF%BC%8C%E9%82%A3%E4%B9%88%E9%87%8C%E9%9D%A2%E7%9A%84%E5%86%85%E5%AE%B9%E5%9C%A8%E6%89%93%E5%8C%85%E6%97%B6%E9%83%BD%E4%BC%9A%E8%A2%AB%E6%89%93%E5%88%B0%E5%8F%91%E5%B8%83%E5%8C%85%E4%B8%AD%E3%80%82,%E6%96%87%E4%BB%B6%E5%A4%B9%E7%89%B9%E7%82%B9%EF%BC%9A%20%E5%8F%AA%E8%AF%BB%EF%BC%8C%E5%8D%B3%E4%B8%8D%E8%83%BD%E5%8A%A8%E6%80%81%E4%BF%AE%E6%94%B9%E3%80%82%20%E6%89%80%E4%BB%A5%E6%83%B3%E8%A6%81%E5%8A%A8%E6%80%81%E6%9B%B4%E6%96%B0%E7%9A%84%E8%B5%84%E6%BA%90%E4%B8%8D%E8%A6%81%E6%94%BE%E5%9C%A8%E8%BF%99%E9%87%8C%E3%80%82%20%E4%BC%9A%E5%B0%86%E6%96%87%E4%BB%B6%E5%A4%B9%E5%86%85%E7%9A%84%E8%B5%84%E6%BA%90%E6%89%93%E5%8C%85%E9%9B%86%E6%88%90%E5%88%B0.asset%E6%96%87%E4%BB%B6%E9%87%8C%E9%9D%A2%E3%80%82
        /// </summary>
        /// <summary>路径相关的实用函数。</summary>
        public static class Path
        {
            // 资源路径
            public const string AB_RESOURCES = "AB_Resources";

            /// <summary>
            /// 得到 AB 资源的输入目录
            /// </summary>
            /// <returns></returns>
            public static string GetABResourcesPath()
            {
                return Application.dataPath + "/" + AB_RESOURCES;
            }

            /// <summary>
            /// 获得 AB 包输出路径
            ///     1\ 平台(PC/移动端等)路径
            ///     2\ 平台名称
            /// </summary>
            /// <returns></returns>
            public static string GetABOutPath()
            {
                return GetPlatformStreamingPath() + "/" + GetPlatformName();
            }

            public static string GetABOutManifestPath()
            {
                return GetABOutPath() + "/" + GetPlatformName();
            }


            public static string GetPlatformDataPath()
            {
                string strReturenPlatformPath = string.Empty;

#if UNITY_EDITOR
                strReturenPlatformPath = Application.dataPath;
#elif UNITY_STANDALONE_WIN
            strReturenPlatformPath = Application.dataPath;

#elif UNITY_IPHONE
                        strReturenPlatformPath = Application.persistentDataPath;
#elif UNITY_ANDROID
                        strReturenPlatformPath = Application.persistentDataPath;
#endif
                return strReturenPlatformPath;
            }

            /// <summary>
            /// 获取平台路径
            /// </summary>
            /// <returns></returns>
            public static string GetPlatformStreamingPath()
            {
                string strReturenPlatformPath = string.Empty;

#if UNITY_STANDALONE_WIN
                strReturenPlatformPath = Application.streamingAssetsPath;

#elif UNITY_IPHONE
                        strReturenPlatformPath = Application.persistentDataPath;
#elif UNITY_ANDROID
                        strReturenPlatformPath = Application.persistentDataPath;
#endif
                return strReturenPlatformPath;
            }

            /// <summary>
            /// 获得平台名称
            /// </summary>
            /// <returns></returns>
            public static string GetPlatformName()
            {
                string strReturenPlatformName = string.Empty;

#if UNITY_STANDALONE_WIN

                strReturenPlatformName = "WindowsABAssets";

#elif UNITY_IPHONE
            strReturenPlatformName = "IPhoneABAssets";
#elif UNITY_ANDROID
            strReturenPlatformName = "AndroidABAssets";
#endif


                return strReturenPlatformName;
            }

            /// <summary>
            /// 返回 WWW 下载 AB 包加载路径
            /// </summary>
            /// <returns></returns>
            public static string GetWWWAssetBundlePath()
            {
                string strReturnWWWPath = string.Empty;

#if UNITY_STANDALONE_WIN
                strReturnWWWPath = "file://" + GetABOutPath();

#elif UNITY_IPHONE
            strReturnWWWPath = GetABOutPath() + "/Raw/";
#elif UNITY_ANDROID
            strReturnWWWPath = "jar:file://" + GetABOutPath();
#endif

                return strReturnWWWPath;
            }

            /// <summary>获取规范的路径。</summary>
            /// <param name="path">要规范的路径。</param>
            /// <returns>规范的路径。</returns>
            public static string GetRegularPath(string path)
            {
                return path?.Replace('\\', '/');
            }

            /// <summary>获取远程格式的路径（带有file:// 或 http:// 前缀）。</summary>
            /// <param name="path">原始路径。</param>
            /// <returns>远程格式路径。</returns>
            public static string GetRemotePath(string path)
            {
                string regularPath = Utility.Path.GetRegularPath(path);
                if (regularPath == null)
                    return (string) null;
                return !regularPath.Contains("://")
                    ? ("file:///" + regularPath).Replace("file:////", "file:///")
                    : regularPath;
            }

            /// <summary>移除空文件夹。</summary>
            /// <param name="directoryName">要处理的文件夹名称。</param>
            /// <returns>是否移除空文件夹成功。</returns>
            public static bool RemoveEmptyDirectory(string directoryName)
            {
                if (string.IsNullOrEmpty(directoryName))
                    throw new UnitFrameworkException();
                try
                {
                    if (!Directory.Exists(directoryName))
                        return false;
                    string[] directories = Directory.GetDirectories(directoryName, "*");
                    int length = directories.Length;
                    foreach (string directoryName1 in directories)
                    {
                        if (Utility.Path.RemoveEmptyDirectory(directoryName1))
                            --length;
                    }

                    if (length > 0 || Directory.GetFiles(directoryName, "*").Length != 0)
                        return false;
                    Directory.Delete(directoryName);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}