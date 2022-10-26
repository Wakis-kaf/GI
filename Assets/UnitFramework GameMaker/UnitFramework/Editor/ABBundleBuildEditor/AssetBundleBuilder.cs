using UnityEditor;
using System.IO;
using System.Linq;
using UnitFramework.Editor;
using UnitFramework.Utils;
using UnityEngine;
namespace UnitFramework.Editor
{
    public class AssetBundleBuilder : UnityEditor.Editor
    {

        public static string[] assetBundleInitDirPath = new string[]
        {
            "ConfigBytes/",
            "Data/",
            "LuaScripts/",
            "UI/",
        };
        [MenuItem("UnitFramework/AssetBundleTools/Create  AssetBundle Resource Directory")]
        public static void CreateABResourceDirectory()  
        {
            CreateABResourceDirectory(assetBundleInitDirPath);  
        }
        public static void CreateABResourceDirectory(string[] assetBundleInitDirPath)
        {
            string path = Utility.Path.GetABResourcesPath();
            if (!Directory.Exists(path)) 
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
            // 初始目录检测 
            for (int i = 0; i < assetBundleInitDirPath.Length; i++)
            {
                string initDirPath = assetBundleInitDirPath[i];
                RecursionCreateDirPath(path, initDirPath);
            }
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// 递归创建AB 包打包资源
        /// </summary>
        private static void RecursionCreateDirPath(string root,string dirPathStr)
        {
            if(string.IsNullOrEmpty(root) || string.IsNullOrEmpty(dirPathStr)) return;
            string[] dirPaths = dirPathStr.Split('/');
            string current = dirPaths[0];
            string path = Path.Combine(root, current);
            string nextDirPathStr = string.Join("/",dirPaths.Where((val, idx) => idx != 0).ToArray());
            Debug.LogFormat("递归创建文件目录 {0} ------- {1}",path,nextDirPathStr);
            if (!Directory.Exists(path)) 
            {
                Directory.CreateDirectory(path);
                
            }
            // 递归创建目录
            RecursionCreateDirPath(path,nextDirPathStr);
        }

        [MenuItem("UnitFramework/AssetBundleTools/CreatAssetBundle for Android")]
        public static void BuildAssetBundleForAndroid()
        {

            //string path = "Assets/StreamingAssets";
            string path = Utility.Path.GetABOutPath();
            BuildAssetBundleForAndroid(path);
        }
        public static void BuildAssetBundleForAndroid(string path)
        {
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.Android);
            UnityEngine.Debug.Log("Android Finish!");
            // refresh asset database
            AssetDatabase.Refresh();
        }


        [MenuItem("UnitFramework/AssetBundleTools/CreatAssetBundle for IOS")]
        public static void BuildAssetBundleForIos()
        {
            //string dirName = "AssetBundles/IOS";
            string path = Utility.Path.GetABOutPath();
            BuildAssetBundleForIos(path);

        }

        public static void BuildAssetBundleForIos(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.CollectDependencies, BuildTarget.iOS);
            UnityEngine.Debug.Log("IOS Finish!");
            // refresh asset database
            AssetDatabase.Refresh();
        }

        [MenuItem("UnitFramework/AssetBundleTools/CreatAssetBundle for Win")]
        public static void BuildAssetBundleForWindows()
        {
            //string path = "AB";
            string path = Utility.Path.GetABOutPath();
            // UnityEngine.Debug.Log(Utility.Path.GetABResourcesPath());
            // UnityEngine.Debug.Log(Utility.Path.GetABOutPath());
            // UnityEngine.Debug.Log(Utility.Path.GetPlatformStreamingPath());
            // UnityEngine.Debug.Log(Utility.Path.GetPlatformName());
            BuildAssetBundleForWindows(path);
        }

        public static void BuildAssetBundleForWindows(string path)
        {
          
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
            UnityEngine.Debug.Log("Windows Finish!");
            // refresh asset database 
            AssetDatabase.Refresh();
        }
       


    }
}
