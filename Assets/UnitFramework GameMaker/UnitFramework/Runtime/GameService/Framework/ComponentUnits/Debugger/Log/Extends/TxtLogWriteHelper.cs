using System;
using System.IO;
using UnitFramework.Utils;
using UnityEditor;
using UnityEngine;

namespace UnitFramework.Runtime
{
    [AutoRegisterHelper]
    public class TxtLogWriteHelper : ILogWriteHelper
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        private string m_logFileName;

        private bool m_logClearChecked = false;
        private bool m_systemInfoLoged = false;
        public TxtLogWriteHelper()
        {
         
            m_logFileName = DateTime.Now.GetDateTimeFormats('s')[0].ToString();
            m_logFileName = m_logFileName.Replace("-", "_");
            m_logFileName = m_logFileName.Replace(":", "_");
            m_logFileName = m_logFileName.Replace(" ", "");
            m_logFileName = m_logFileName.Replace("T", "_");
            m_logFileName = m_logFileName +".log";
        }
        public void WriteLog(Log.LogData logData,ILogWriter writer)
        {
          
            // 删除旧日志文件
            if (!m_logClearChecked)
            {
                LogClear(writer);
                m_logClearChecked = true;
            }
            // 写入文件
            WriteLog(writer, logData);
        }

        private void LogClear(ILogWriter writer)
        {
            string fileDir =Utility.Path.GetPlatformStreamingPath() + writer.LogWritePath;
            // 如果根目录不存在就退出
            if (!Directory.Exists(fileDir))
            {
                return;
            }
            // 获取目录下的所有子文件
            DirectoryInfo directory = new DirectoryInfo(fileDir);
            var files = directory.GetFiles("*");
            // 删除所有的子文件
            if (writer.SaveLogOnlyCurrent)
            {
                //Debug.Log("删除所有子文件");
                foreach (var file in files)
                {
                    file.Delete();
                }
            }
            else if(writer.LogClearOld)
            {
                if (files.Length > writer.LogFileMaxCount)
                {
                    var oldFile = files[0];
                    var lastTime = files[0].CreationTime;
                    foreach (var file in files)
                    {
                        if (lastTime > file.CreationTime)
                        {
                            oldFile = file;
                            lastTime = file.CreationTime;
                        }
                    }
                    oldFile.Delete();
                }
            }
#if  UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
          
        }

        private StreamWriter m_SW;

        private string GetDirPath(SaveDirPath saveDirPath)
        {
            switch (saveDirPath)
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

        private string GetFullDirPath(ILogWriter writer)
        {
            string dirPath = GetDirPath(writer.SaveDirPath);
            string writerPath = writer.LogWritePath;
            if (writerPath.StartsWith("/"))
            {
                writerPath = writerPath.Remove(0, 1);
            }
            string fullPath = Path.Combine(dirPath, writerPath);
            return fullPath;
        }
        private StreamWriter GetStreamWriter(ILogWriter writer)
        {
            if (m_SW == null)
            {
               
                string dirPath = GetFullDirPath(writer);
                string filePath = Path.Combine(dirPath, m_logFileName);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
#if  UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif
                }
             
                m_SW = File.AppendText(filePath);
                m_SW.AutoFlush = true;
            }

            return m_SW;


        }

        private void WriteLog(ILogWriter writer,Log.LogData logData)
        {
            //Debug.Log("Write");
            // 写入文件 
            var sw = GetStreamWriter(writer);
            if (!m_systemInfoLoged)
            {
                WriteSystemInfo(sw);
                m_systemInfoLoged = true;
            }
            sw.WriteLine(logData.GetMessage());
            
        }
        /// <summary>
        /// 写入系统文件
        /// </summary>
        /// <param name="sw"></param>
        private  void WriteSystemInfo(StreamWriter sw)
        {
            sw.WriteLine("*********************************************************************************************************start");
            sw.WriteLine("By " + SystemInfo.deviceName);
            DateTime now = DateTime.Now;
            sw.WriteLine(string.Concat(new object[] { now.Year.ToString(), "年", now.Month.ToString(), "月", now.Day, "日  ", now.Hour.ToString(), ":", now.Minute.ToString(), ":", now.Second.ToString() }));
            sw.WriteLine();
            sw.WriteLine("操作系统:  " + SystemInfo.operatingSystem);
            sw.WriteLine("系统内存大小:  " + SystemInfo.systemMemorySize);
            sw.WriteLine("设备模型:  " + SystemInfo.deviceModel);
            sw.WriteLine("设备唯一标识符:  " + SystemInfo.deviceUniqueIdentifier);
            sw.WriteLine("处理器数量:  " + SystemInfo.processorCount);
            sw.WriteLine("处理器类型:  " + SystemInfo.processorType);
            sw.WriteLine("显卡标识符:  " + SystemInfo.graphicsDeviceID);
            sw.WriteLine("显卡名称:  " + SystemInfo.graphicsDeviceName);
            sw.WriteLine("显卡标识符:  " + SystemInfo.graphicsDeviceVendorID);
            sw.WriteLine("显卡厂商:  " + SystemInfo.graphicsDeviceVendor);
            sw.WriteLine("显卡版本:  " + SystemInfo.graphicsDeviceVersion);
            sw.WriteLine("显存大小:  " + SystemInfo.graphicsMemorySize);
            sw.WriteLine("显卡着色器级别:  " + SystemInfo.graphicsShaderLevel);
            sw.WriteLine("是否支持内置阴影:  " + SystemInfo.supportsShadows);
            sw.WriteLine("*********************************************************************************************************end");
            sw.WriteLine("LogInfo:");
            sw.WriteLine();
        }
        public void Dispose()
        {
            if (!ReferenceEquals(m_SW,null))
            {
                m_SW.Close();
                m_SW.Dispose();    
            }
            m_SW = null;
            m_systemInfoLoged = false;
            m_logClearChecked = false;
        }
    }
}