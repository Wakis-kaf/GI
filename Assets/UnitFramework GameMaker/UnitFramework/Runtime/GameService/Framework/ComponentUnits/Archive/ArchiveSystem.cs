
using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnitFramework.Runtime.Archives;
using UnityEngine;
namespace UnitFramework.Runtime
{
    /// <summary>
    /// 存档系统 对游戏进行存档
    /// </summary>
    public  sealed  partial class ArchiveSystem : SingletonComponentUnit<ArchiveSystem>
    {
        public override string ComponentUnitName { get=>"SaveSystem"; }
        public SaveMode saveMode;
        [TitleGroup("SaveDirPath")]
        [HorizontalGroup("SaveDirPath/SaveMode Group",0.5f),HideLabel]
        public SaveDirPath savePath;
        [HorizontalGroup("SaveDirPath/SaveMode Group",0.5f),HideLabel]
        public string relativePath ="/Archives/"; // 相对路径
        public string archiveSystemVersion = "1.0"; //  版本号
        
        private ArchiveManager m_ArchiveManager;
        private Dictionary<Type, object> m_LoadCallback =  new Dictionary<Type, object>();
        private Dictionary<Type, object> m_SaveCallback = new Dictionary<Type, object>();

        
        /// <summary>
        /// 单元初始化的时候调用
        /// </summary>
        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            m_ArchiveManager = AddChildUnit<ArchiveManager>();
            m_ArchiveManager.SetHelper(ArchiverSerializerFactory.CreatSerializer(saveMode));
        }

        /// <summary>
        /// 注册存档读保存回调事件
        /// </summary>
        /// <param name="readCallBack"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterSave<T>(Action<Archive<T>> saveCallBack)
        {
            var type = typeof(T);
            if (!m_SaveCallback.ContainsKey(type))
            {
                m_SaveCallback.Add(type, saveCallBack);
            }
            else
            {
                Action<Archive<T>> action = (Action<Archive<T>>) m_SaveCallback[type];
                action += saveCallBack;
            }
        }
        /// <summary>
        /// 触发存档保存事件
        /// </summary>
        /// <param name="archive"></param>
        /// <typeparam name="T"></typeparam>
        private void InvokeSave<T>(Archive<T> archive)
        {
            if (m_SaveCallback.TryGetValue(typeof(T),out object actonObj))
            {
                Action<Archive<T>> action = (Action<Archive<T>>) actonObj;
                action?.Invoke(archive);
            }
        }
        /// <summary>
        /// 移除粗胆囊保存回调事件
        /// </summary>
        /// <param name="saveCallBack"></param>
        /// <typeparam name="T"></typeparam>
        public void RemoveSave<T>(Action<Archive<T>> saveCallBack)
        {
            var type = typeof(T);
            if (m_SaveCallback.ContainsKey(type))
            {
                Action<Archive<T>> action = (Action<Archive<T>>) m_SaveCallback[type];
                action -= saveCallBack;
            }
            
        }
        
        
        /// <summary>
        /// 注册存档读取回调事件
        /// </summary>
        /// <param name="loadCallBack"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterLoad<T>(Action<Archive<T>> loadCallBack)
        {
            var type = typeof(T);
            if (!m_LoadCallback.ContainsKey(type))
            {
                m_LoadCallback.Add(type, loadCallBack);
            }
            else
            {
                Action<Archive<T>> action = (Action<Archive<T>>) m_LoadCallback[type];
                action += loadCallBack;
            }
        }
        /// <summary>
        /// 触发存档读取事件
        /// </summary>
        /// <param name="archive"></param>
        /// <typeparam name="T"></typeparam>
        private void InvokeLoad<T>(Archive<T> archive)
        {
            if (m_LoadCallback.TryGetValue(typeof(T),out object actonObj))
            {
                Action<Archive<T>> action = (Action<Archive<T>>) actonObj;
                action?.Invoke(archive);
            }
        }
        /// <summary>
        /// 移除存档读取事件
        /// </summary>
        /// <param name="loadCallBack"></param>
        /// <typeparam name="T"></typeparam>
        public void RemoveLoad<T>(Action<Archive<T>> loadCallBack)
        {
            var type = typeof(T);
            if (m_LoadCallback.ContainsKey(type))
            {
                Action<Archive<T>> action = (Action<Archive<T>>) m_LoadCallback[type];
                action -= loadCallBack;
            }
            
        }
        
        /// <summary>
        /// 创建一个存档
        /// </summary>
        public Archive<T> CreateArchive<T>(string archiveName = "")
        {
            Archive<T> archive = new Archive<T>(archiveSystemVersion,archiveName); // 新建一个存档
            SaveArchive<T>(archive, false);
            return archive;
        }
        /// <summary>
        /// 创建一个存档
        /// </summary>
        public Archive<T> CreateArchive<T>(T data,string archiveName = "")
        {
            Archive<T> archive = new Archive<T>(data,archiveSystemVersion,archiveName); // 新建一个存档
            SaveArchive<T>(archive, false);
            return archive;
        }
        
        /// <summary>
        ///  保存存档
        /// </summary>
        /// <param name="archive"></param>
        public bool SaveArchive<T>(Archive<T> archive,bool isOverride = true)
        {
            string archiveFileName = archive.GetArchiveFileName();
            string archiveFullPath = string.Empty;
            string archiveDirPath = string.Empty;
            if (HasArchive(archiveFileName,ref archiveFullPath,ref archiveDirPath))
            {
                if(!isOverride)
                {
                    Log.ErrorFormat("Save Archive Error! Target has already exist ! {0}",
                        archiveFileName);
                    return false;
                }
            }

            // 创建文件
            if (!Directory.Exists(archiveDirPath))
            {
                Directory.CreateDirectory(archiveDirPath);
            }
            //  覆盖 或者创建 存档
            using (FileStream fs = new FileStream(archiveFullPath, FileMode.Create))
            {
                // 序列化存档中的自定义数据
                //m_ArchiveManager.Serialize(archive);
                // 序列化数据到流中
                archive.OnSerialize();
                Log.DebugInfo($"Save Archive Path : {archiveFullPath}");
                // 触发事件
                InvokeSave(archive);
                var res = m_ArchiveManager.Serialize(fs, archive);
                if (res)
                {
#if  UNITY_EDITOR 
                    // 编辑器下刷新 
                    UnityEditor.AssetDatabase.Refresh();   
#endif
                }
                    
                return res;
            }
            
            
        }
        
        /// <summary>
        /// 加载存档
        /// </summary>
        /// <param name="archiveFileName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Archive<T> LoadArchive<T>(string archiveFileName)
        {
            string archiveFullPath = string.Empty;
            string archiveDirPath = string.Empty;
            if (!HasArchive(archiveFileName, ref archiveFullPath,ref archiveDirPath))
            {
                Log.ErrorFormat(" Archive Not Exist ! {0}", archiveFullPath);
                return null;
            }
            using (FileStream fs = new FileStream(archiveFullPath, FileMode.Open,FileAccess.Read))
            {
                var archive = m_ArchiveManager.DeSerialize<Archive<T>>(fs);
                archive.OnDeSerialize();
                InvokeLoad(archive);
                return archive;
            }
          
        }
        /// <summary>
        /// 加载存档
        /// </summary>
        /// <param name="archiveFileName"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Archive<T> LoadArchive<T>(string archiveFileName,out T data)
        {
            var archive = LoadArchive<T>(archiveFileName);
            data = archive.GetData();
            return archive;
        }
        /// <summary>
        /// 判断存档文件是否存在
        /// </summary>
        /// <param name="archiveFileName"></param>
        /// <returns></returns>
        public bool HasArchive(string archiveFileName)
        {
            string archiveFullPath = string.Empty;
            string archiveDirPath = string.Empty;
            return HasArchive(archiveFileName, ref archiveFullPath,ref archiveDirPath);
        }
        /// <summary>
        /// 判断存档文件是否存在 
        /// </summary>
        /// <param name="archiveFileName"></param>
        /// <param name="archiveFullPath"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool HasArchive(string archiveFileName,ref string archiveFullPath,ref  string fileDirPath)
        {
            string filePath = relativePath;
           
            if (filePath.StartsWith("/") || filePath.StartsWith("\\"))
                filePath = filePath.Substring(1, filePath.Length - 1);
            string path = Path.Combine(GetDirPath(), filePath);
            archiveFullPath = path;
            fileDirPath = path;
            // 判断路径是否存在
            if (!Directory.Exists(path))
            {
                return false;
            }
            string archiveName = archiveFileName + ArchiveFilePrefix.GetPrefix(saveMode); // 获取存档名
            string fullPath = Path.Combine(path, archiveName);
            archiveFullPath = fullPath;
            if (!File.Exists(fullPath)){
                return false;
            }
          
           
            
            return true;
        }
  
        /// <summary>
        /// 获取目录路径
        /// </summary>
        /// <returns></returns>
        private string GetDirPath()
        {
            switch (savePath)
            {
                case  SaveDirPath.DataPath:
                    return Application.dataPath;
                case SaveDirPath.PersistencePath :
                    return Application.persistentDataPath;
                case SaveDirPath.StreamingPath:
                    return Application.streamingAssetsPath;  
                case SaveDirPath.TemporaryCachePath:
                    return Application.temporaryCachePath;
                  
            }

            return string.Empty;
        }
     
    }
    

}
