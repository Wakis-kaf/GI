using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 日志等级
    /// </summary>
    /// 
    [Flags]
    public enum LogLevel
    {
        DEBUG = 1 << 1, // 开发过程中的一些运行信息
        INFO = 1 << 2, // 一些重要的或者比较感兴趣的信息，用于生产环境中的一些重要信息
        WARN = 1 << 3, // 警告信息用于提供一些提示
        ERROR = 1 << 4, // 显示发生错误信息，但不影响系统的继续运行打印错误和异常信息
        FATAL = 1 << 5, // 指出每个严重的错误事件将会导致应用程序的退出，重大错误，可以直接停止程序；
        //ALL = DEBUG | INFO | WARN | ERROR | FATAL
    }

    /// <summary>
    /// 日志配置文件
    /// </summary>
    [System.Serializable]
    public class EkfLogConfig
    {
        [Header("是否开启UnityLog")] public bool IsReceiveUnityLog = false;
        [Header("日志存储信息容量，-1为无限制")] public int LogQueueCapacity = 1000; // 日志容量， -1 表示无限制
        [Header("日志缓存队列容量")] public int LogCacheQueueCount = 100;
        [Header("日志缓存每帧最大处理数量")] public int LogCacheHandleMaxCount = 100;
        [Header("允许输出日志等级")] public LogLevel EnableLevel = (LogLevel) (~0);
        [Header("允许打印日志等级")] public LogLevel EnableLogLevel = (LogLevel) (~0);
        [Header("允许Write日志等级")] public LogLevel EnableWriteLevel = (LogLevel) (~0);
        [Header("允许输出日志时间")] public LogLevel EnableTimeLevel = LogLevel.FATAL | LogLevel.ERROR;
        [Header("堆栈输出允许等级")] public LogLevel EnableTrackLevel = (LogLevel) (~0);
        [Header("输出过滤")] public List<string> MessageFilterStrs = new List<string>();

        public static string InfoPrefix = "[UNIT FRAMEWORK LOG] ";
    }


    [LogStackTraceIgnore]
    [DisallowMultipleComponent]
    [AddComponentMenu("EKaf Framework/LogComponent")]
    public class Log : SingletonComponentUnit<Log>, ILogWriter 
    {
        public class LogData
        {
            public LogLevel logLevel; // 日志级别
            public string logTime; // 日志时间
            //public int LogFrameCount; // 日志时间
            public string logPrefix;
            public object logMessageObject; // 日志消息
            //public string logMessage;
            public string logBasicData; // 日志基础数据
            public string logTrack; // 日志堆栈信息
            public string GetMessage()
            {
                string track = Log.HasInstance() ? logTrack : string.Empty;
                return string.Concat(EkfLogConfig.InfoPrefix, logTime, logPrefix, logMessageObject, track);
            }

            public static LogData CreateFromUnityLog(string condition, string stacktrace, LogType type)
            {
                LogLevel logLevel = LogLevel.INFO;
                switch (type)
                {
                    case LogType.Error:
                        logLevel = LogLevel.ERROR;
                        break;
                    case LogType.Exception:
                        logLevel = LogLevel.FATAL;
                        break;
                    case LogType.Log:
                        logLevel = LogLevel.INFO;
                        break;
                    case LogType.Warning:
                        logLevel = LogLevel.WARN;
                        break;
                    case LogType.Assert:
                        logLevel = LogLevel.DEBUG;
                        break;
                }

                if (condition.StartsWith(EkfLogConfig.InfoPrefix) && condition.Contains(LogLevelPrefix[LogLevel.DEBUG]))
                {
                    logLevel = LogLevel.DEBUG;
                }
                LogData data = new LogData();
                data.logMessageObject = condition;
                data.logTrack = stacktrace;
                data.logLevel = logLevel;
                data.logPrefix = GetLevelPrefix(logLevel); // 获取前缀
                
                if (IsLogTime(logLevel))
                    data.logTime = DateTime.Now.ToString("HH:mm:ss");
                return data;
            }
            
        }
        private class LogTraceIgnore
        {
           
            public string logTypeName = "";
            public LogTraceIgnore(System.Type logType)  
            {  
              
                this.logTypeName = logType.FullName +":";  
            }  
        }
        private static LogTraceIgnore[] mLogTraceIgnore = new LogTraceIgnore[]   
        {  
#if UNITY_EDITOR
            new LogTraceIgnore(typeof(UnityLogPrintHelper)),
#endif
            new LogTraceIgnore(typeof(Log))  
        }; 
        // 日志输出前缀
        public static Dictionary<LogLevel, string> LogLevelPrefix =
            new Dictionary<LogLevel, string>()
            {
                {LogLevel.DEBUG, " [DEBUG]: "},
                {LogLevel.INFO, " [INFO]: "},
                {LogLevel.WARN, " [WARN]: "},
                {LogLevel.ERROR, " [ERROR]: "},
                {LogLevel.FATAL, " [FATAL]: "},
            };

        public static event Action<LogData> OnLogDataReceived;
        public static string GetLevelPrefix(LogLevel logLevel)
        {
            return LogLevelPrefix[logLevel];
        }
        
        public override string ComponentUnitName
        {
            get => "EKafFramework DebugInfo";
        }

        public string LogWritePath
        {
            get => m_LogWriteDirPath;
            set { m_LogWriteDirPath = value; }
        }

        public SaveDirPath SaveDirPath
        {
            get => saveDirPath;
        }
        public bool SaveLogOnlyCurrent
        {
            get => m_SaveLogOnlyCurrent;
            set => m_SaveLogOnlyCurrent = value;
        }

        public bool LogClearOld
        {
            get => m_LogClearOld;
            set => m_LogClearOld = value;
        }

        public int LogFileMaxCount
        {
            get => m_LogFileMaxCount;
            set => m_LogFileMaxCount = value;
        }

        public static int LogDatasCount
        {
            get => m_LogDatas.Count;
        }
        
        /// <summary>
        /// 配置文件
        /// </summary>
        public EkfLogConfig EkfLogConfig;

        private static Queue<LogData> m_LogDatas = new Queue<LogData>(1000);
        private static Queue<LogData> m_CachePrintQueue = new Queue<LogData>(100);
        private static Queue<LogData> m_CacheWriteQueue = new Queue<LogData>(100);
        [TitleGroup("日志写入路径")]
        [HorizontalGroup("日志写入路径/SaveMode Group",0.5f),HideLabel]
        public SaveDirPath saveDirPath;
        [HorizontalGroup("日志写入路径/SaveMode Group",0.5f),HideLabel]
        [SerializeField] private string m_LogWriteDirPath = "/GameLogs/"; // 相对路径
        //[Header("日志写入路径")] [SerializeField] private string m_LogWriteStreamingPath = "/GameLogs/";
        [Header("是否只保留本次日志")] [SerializeField] private bool m_SaveLogOnlyCurrent = false;
        [Header("是否清除旧日志")] [SerializeField] private bool m_LogClearOld = true;
        [Header("旧日志保留上限")] [SerializeField] private int m_LogFileMaxCount = 20;

        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            //EKFLog.Info("EKFLog OnUnitAwake");
            //m_LogWriteStreamingPath = PathUtility.GetPlatformDataPath().Replace("Assets", "") + "GameLogs";
            if (Instance.EkfLogConfig.IsReceiveUnityLog)
            {
                Application.logMessageReceived += UnityInternalLog;
            }

            StartCoroutine(Coroutine_LogCacheHandling());
// #if UNITY_EDITOR
//             EditorAssetInit();
// #endif
        }

        IEnumerator Coroutine_LogCacheHandling()
        {
            while (true)
            {
                TryLogCache();
                yield return null;
            }
        }

        private void TryLogCache()
        {
            int count = m_CachePrintQueue.Count;
            if (count != 0 && TryGetPrintExtends(out var logExtends))
            {
                count = Mathf.Min(count, Instance.EkfLogConfig.LogCacheHandleMaxCount);
                for (int i = 0; i < count; i++)
                {
                    var data = m_CachePrintQueue.Dequeue();
                    foreach (var logExtend in logExtends)
                    {
                        // 打印日志
                        logExtend.PrintLog(data);
                    }
                }
            }


            count = m_CacheWriteQueue.Count;
            if (count != 0 && TryGetWriteExtends(out var writeExtends))
            {
                count = Mathf.Min(count, Instance.EkfLogConfig.LogCacheHandleMaxCount);
                for (int i = 0; i < count; i++)
                {
                    var data = m_CacheWriteQueue.Dequeue();
                    foreach (var logExtend in writeExtends)
                    {
                        // 写入日志
                        logExtend.WriteLog(data, Instance);
                    }
                }
            }
        }

        private static void UnityInternalLog(string condition, string stacktrace, LogType type)
        {
            if (condition.StartsWith(EkfLogConfig.InfoPrefix)) return;
            LogLevel logLevel = LogLevel.INFO;
            switch (type)
            {
                case LogType.Error:
                    logLevel = LogLevel.ERROR;
                    break;
                case LogType.Exception:
                    logLevel = LogLevel.FATAL;
                    break;
                case LogType.Log:
                    logLevel = LogLevel.INFO;
                    break;
                case LogType.Warning:
                    logLevel = LogLevel.WARN;
                    break;
                case LogType.Assert:
                    logLevel = LogLevel.DEBUG;
                    break;
            }

            RecordLog(condition, logLevel, stacktrace);
        }

        private static bool TryGetPrintExtends(out ILogPrintHelper[] extends)
        {
            extends = null;
            if (ReferenceEquals(GameFramework.Instance, null)
                || ReferenceEquals(GameFramework.Container, null))
                return false;
            extends = GameFramework.Container.GetHelpers<ILogPrintHelper>();
            if (extends == null || extends.Length <= 0)
            {
                return false;
            }

            return true;
        }

        private static bool TryGetWriteExtends(out ILogWriteHelper[] extends)
        {
            extends = null;
            if (ReferenceEquals(GameFramework.Instance, null)
                || ReferenceEquals(GameFramework.Container, null))
                return false;
            extends = GameFramework.Container.GetHelpers<ILogWriteHelper>();
            if (extends == null || extends.Length <= 0)
            {
                return false;
            }

            return true;
        }

        private static void LogDataLog(LogData data)
        {
            if (TryGetPrintExtends(out ILogPrintHelper[] extends))
            {
                foreach (var logExtend in extends)
                {
                    // 打印日志
                    logExtend.PrintLog(data);
                }
            }
            else
            {
                if (HasInstance() && m_CachePrintQueue.Count >= Instance.EkfLogConfig.LogCacheQueueCount)
                {
                    m_CachePrintQueue.Dequeue();
                }

                m_CachePrintQueue.Enqueue(data);
            }
        }

        private static void LogDataWrite(LogData data)
        {
            if (HasInstance() && TryGetWriteExtends(out ILogWriteHelper[] extends))
            {
                foreach (var logExtend in extends)
                {
                    // 写入日志
                    logExtend.WriteLog(data, Instance);
                }
            }
            else
            {
                if (HasInstance() && m_CacheWriteQueue.Count >= Instance.EkfLogConfig.LogCacheQueueCount)
                {
                    m_CacheWriteQueue.Dequeue();
                }

                m_CacheWriteQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// 获取堆栈信息
        /// </summary>
        /// <returns></returns>
        private static string GetTrackInfo()
        {
            string track = "\n";
            //把无关的log去掉
            var st = StackTraceUtility.ExtractStackTrace();
            for (int i = 0; i < 1; i++)
            {
                st = st.Remove(0, st.IndexOf('\n') + 1);
            }
           
            bool isFound = false;
            do
            {
                int index = st.IndexOf('\n') + 1;
                isFound = false;
                string line = st.Substring(0, index);
                //Debug.Log("Line"+line);
                for (int j = 0; j < mLogTraceIgnore.Length; j++)
                {
                    if (line.Contains(mLogTraceIgnore[j].logTypeName))
                    {
                        st = st.Remove(0, index);
                        isFound = true;
                        break;
                    }
                }
            } while (isFound);
         

            return st;
        }

        private static bool IsEnableLogLevel(LogLevel logLevel)
        {
            if (!HasInstance()) return true;
            if ((Instance.EkfLogConfig.EnableLogLevel & logLevel) != 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsEnableWriteLevel(LogLevel logLevel)
        {
            if (!HasInstance()) return true;
            if ((Instance.EkfLogConfig.EnableWriteLevel & logLevel) != 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsLogTime(LogLevel logLevel)
        {
            if (!HasInstance()) return true;
            if ((Instance.EkfLogConfig.EnableTimeLevel & logLevel) != 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsLogTrack(LogLevel logLevel)
        {
            if (!HasInstance()) return true;
            if ((Instance.EkfLogConfig.EnableTrackLevel & logLevel) != 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsIgnoreLevel(LogLevel logLevel)
        {
            if (!HasInstance()) return false;
            if ((Instance.EkfLogConfig.EnableLevel & logLevel) != 0)
            {
                return false;
            }

            return true;
        }

        private static bool IsIgnoreMessage(object message)
        {
            if (!HasInstance()) return false;
            string content = message.ToString();
            if (string.IsNullOrEmpty(content)) return true;
            int count = Instance.EkfLogConfig.MessageFilterStrs.Count;
            for (int i = 0; i < count; i++)
            {
                if (content.ToLower().Contains(Instance.EkfLogConfig.MessageFilterStrs[i].ToLower())) return true;
            }

            return false;
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        /// <param name="track"></param>
       
        protected static void RecordLog(object message, LogLevel logLevel = LogLevel.INFO, string track = "")
        {
            if (IsIgnoreLevel(logLevel) || IsIgnoreMessage(message)) return;
            if (IsLogTrack(logLevel))
            {
#if !UNITY_EDITOR || ENABLE_LOG_TRACE
                if (string.IsNullOrEmpty(track))
                {
                    track = GetTrackInfo();
                }
#endif
            }
            else
            {
                track = String.Empty;
            }

            if (HasInstance())
            {
                Instance.TryLogCache();
            }

            LogData data = new LogData();
            data.logMessageObject = message;
            data.logTrack = track;
            data.logLevel = logLevel;
            data.logPrefix = GetLevelPrefix(logLevel); // 获取前缀
            //data.LogFrameCount = Time.frameCount;
            if (IsLogTime(logLevel))
                data.logTime = DateTime.Now.ToString("HH:mm:ss");
            
            // 保存日志信息
            if (HasInstance() && Instance.EkfLogConfig.LogQueueCapacity <= m_LogDatas.Count)
            {
                m_LogDatas.Dequeue();
            }

            m_LogDatas.Enqueue(data);
            OnLogDataReceived?.Invoke(data);
            if (IsEnableLogLevel(logLevel))
            {
                LogDataLog(data);
            }

            if (IsEnableWriteLevel(logLevel))
            {
                LogDataWrite(data);
            }
        }

        //[LogStackTraceIgnore]
        public static void Info(object message)
        {
            RecordLog(message, LogLevel.INFO);
        }

        public static void InfoFormat(string msg, params object[] objs)
        {
            string message = string.Format(msg, objs);
            RecordLog(message, LogLevel.INFO);
        }

        //[LogStackTraceIgnore]
        public static void Warning(object message)
        {
            RecordLog(message, LogLevel.WARN);
        }

        public static void WarnFormat(string msg, params object[] objs)
        {
            string message = string.Format(msg, objs);
            RecordLog(message, LogLevel.WARN);
        }

        //[LogStackTraceIgnore]
        public static void DebugInfo(object message)
        {
            RecordLog(message, LogLevel.DEBUG);
        }

        public static void DebugInfoFormat(string msg, params object[] objs)
        {
            string message = string.Format(msg, objs);
            RecordLog(message, LogLevel.DEBUG);
        }

        //[LogStackTraceIgnore]
        public static void Error(object message)
        {
            RecordLog(message, LogLevel.ERROR);
        }

        public static void ErrorFormat(string msg, params object[] objs)
        {
            string message = string.Format(msg, objs);
            RecordLog(message, LogLevel.ERROR);
        }

        //[LogStackTraceIgnore]
        public static void Fatal(object message)
        {
            RecordLog(message, LogLevel.FATAL);
        }

        public static void FatalFormat(string msg, params object[] objs)
        {
            string message = string.Format(msg, objs);
            RecordLog(message, LogLevel.FATAL);
        }


        public static bool HasInstance()
        {
            return !ReferenceEquals(Log.Instance, null);
        }

        public static LogData GetLogDataAt(int index)
        {
            if (index >= m_LogDatas.Count) return null;
            return m_LogDatas.ElementAt(index);
        }

    }
} // #if UNITY_EDITOR
//         private void EditorAssetInit()
//         {
//             UnityEngine.Object debuggerFile = AssetDatabase.LoadAssetAtPath(Debugerfilepath, typeof(UnityEngine.Object));
//             m_DebugerFileInstanceId = debuggerFile.GetInstanceID();
//             m_ConsoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");//获取consoleWindow
//             m_ActiveTextInfo = m_ConsoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);//是当前被选中的Log的详细信息
//             m_ConsoleWindowFileInfo = m_ConsoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);//consoleWindow 界面实例
//         }
//
//         [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
//         private static bool OnOpenAsset(int instanceID, int line)
//         {
//             if (Instance == null) return false;
//             if (instanceID == Instance.m_DebugerFileInstanceId || IsPritntExtend(instanceID))//双击判断对应得log封装类是否是指定封装类
//             {
//                 //Debug.Log("Find code");
//                 return Instance.FindCode();
//             }
//             return false;
//         }
//         private static bool IsPritntExtend(int instanceID)
//         {
//             var obj = EditorUtility.InstanceIDToObject(instanceID);
//             foreach (var logExtend in EKafEntry.Extend.GetExtends<IEKFLogPrintExtend>())
//             {
//                 if (logExtend.GetType().Name.Contains(obj.name))
//                 {
//                     return true;
//                 }
//             }
//             return false;
//         }
//         public bool FindCode()
//         {
//             var windowInstance = m_ConsoleWindowFileInfo.GetValue(null);
//             var activeText = m_ActiveTextInfo.GetValue(windowInstance);
//             string[] contentStrings = activeText.ToString().Split('\n');
//             List<string> filePath = new List<string>();
//             for (int index = 0; index < contentStrings.Length; index++)
//             {
//                 if (contentStrings[index].Contains("at "))
//                 {
//                     //Debug.Log(contentStrings[index]);
//                     filePath.Add(contentStrings[index]);
//                 }
//             }
//             //bool success = PingAndOpen(filePath[1]);
//             bool success = Open2(filePath[3]);
//             return success;
//         }
//         public bool Open(List<string> filePaths)
//         {
//             string regexRule = @"\(at(.+)\)";   // 正则表达格式     
//             
//             for (int i = 0; i < filePaths.Count; i++)
//             {
//                 string message = filePaths[i];
//                 // 正则表达式匹配路径
//                 Match matches = Regex.Match(regexRule, regexRule, RegexOptions.IgnoreCase);
//                 // 获取 该类类型
//                 int idx01 = message.IndexOf(":");
//                 int idx02 = message.IndexOf("(");
//                 int idx03 = message.IndexOf(")");
//                 string classTypeStr = message.Substring(0, idx01);
//                
//                 // 获取 该方法类型
//                 string methodNameStr = message.Substring(idx01+1, idx02-idx01-1);
//                 Debug.Log("methodName" + methodNameStr);
//                 // 获取参数类型
//                 string arsStr = message.Substring(idx02+1, idx03 - idx02-1);
//
//                 Type classType = Type.GetType(classTypeStr);
//                 Debug.Log("classType" + classType);
//                 Debug.Log("ars" + arsStr);
//                 string[] arsStrs = arsStr.Split(',');
//                 Type[] argsType = new Type[arsStrs.Length];
//                 for (int j = 0; j < arsStrs.Length; j++)
//                 {
//                    
//                     argsType[j] = Type.GetType(arsStrs[j]);
//                     Debug.Log(argsType[j]);
//                 }
//                 MethodInfo methodInfo = classType.GetMethod("PrintLog", BindingFlags.Instance 
//                     | BindingFlags.Public |
//                     BindingFlags.Static | 
//                     BindingFlags.NonPublic);
//                 Debug.Log(methodInfo);
//                 if (!Attribute.IsDefined(methodInfo, typeof(LogStackTraceIgnoreAttribute))){
//                     // TODO:
//                     Debug.Log("跳转" + filePaths[i]);
//                 }
//                
//             }
//             return false;
//             
//
//            
//         }
//         public bool Open2(string fileContext)
//         {
//             string regexRule = @"Assets\b([\w\W]*):(\d+)";   // 正则表达格式    
//             Match match = Regex.Match(fileContext, regexRule);
//             if (match.Groups.Count > 1)
//             {
//                 string path = "Assets" + match.Groups[1].Value;
//                 string line = match.Groups[2].Value;
//                 //Debug.Log($"path{path} line{line} ");
//                 UnityEngine.Object codeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
//                 if (codeObject == null)
//                 {
//                     return false;
//                 }
//                 EditorGUIUtility.PingObject(codeObject);
//
//                 AssetDatabase.OpenAsset(codeObject, int.Parse(line)); //打开指定脚本 并跳转对应行数           
//                 return true;
//             }
//             return false;
//         }
//         public bool PingAndOpen(string fileContext)
//         {
//             string regexRule = @"Assets\b([\w\W]*):(\d+)";   // 正则表达格式     
//             Match match = Regex.Match(fileContext, regexRule);
//             if (match.Groups.Count > 1)
//             {
//                 string path = "Assets" + match.Groups[1].Value;
//                 string line = match.Groups[2].Value;
//                 UnityEngine.Object codeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
//                 if (codeObject == null)
//                 {
//                     return false;
//                 }
//                 EditorGUIUtility.PingObject(codeObject);
//                 
//                 AssetDatabase.OpenAsset(codeObject, int.Parse(line)); //打开指定脚本 并跳转对应行数           
//                 return true;
//             }
//             return false;
//         }
// #endif