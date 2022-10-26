using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public sealed partial class DebuggerComponent 
    {
        [Serializable]
        private sealed class ConsoleWindow : IDebuggerWindow
        {
            private readonly Queue<Log.LogData> m_LogNodes = new Queue<Log.LogData>();

            private SettingComponent m_SettingComponent = null;
            private Vector2 m_LogScrollPosition = Vector2.zero;
            private Vector2 m_StackScrollPosition = Vector2.zero;
            private int m_DebugCount = 0;
            private int m_InfoCount = 0;
           
            private int m_WarningCount = 0;
            private int m_ErrorCount = 0;
            private int m_FatalCount = 0;
            private Log.LogData m_SelectedNode = null;
            private bool m_LastLockScroll = true;
            private bool m_LastInfoFilter = true;
            private bool m_LastWarningFilter = true;
            private bool m_LastErrorFilter = true;
            private bool m_LastFatalFilter = true;

            [SerializeField]
            private bool m_ReceiveFromUnity = false;
            [SerializeField] 
            private bool m_ReceiveFromCustomLog = true;
            [SerializeField]
            private bool m_LockScroll = true;

            [SerializeField]
            private int m_MaxLine = 100;

            [SerializeField]
            private bool m_DebugFilter = true;
            [SerializeField]
            private bool m_InfoFilter = true;

            [SerializeField]
            private bool m_WarningFilter = true;

            [SerializeField]
            private bool m_ErrorFilter = true;

            [SerializeField]
            private bool m_FatalFilter = true;

            [SerializeField]
            private Color32 m_DebugColor = Color.blue;
            [SerializeField]
            private Color32 m_InfoColor = Color.white;
            [SerializeField]
            private Color32 m_WarningColor = Color.yellow;

            [SerializeField]
            private Color32 m_ErrorColor = Color.red;

            [SerializeField]
            private Color32 m_FatalColor = new Color(0.7f, 0.2f, 0.2f);

            public bool LockScroll
            {
                get
                {
                    return m_LockScroll;
                }
                set
                {
                    m_LockScroll = value;
                }
            }

            public int MaxLine
            {
                get
                {
                    return m_MaxLine;
                }
                set
                {
                    m_MaxLine = value;
                }
            }

            public bool InfoFilter
            {
                get
                {
                    return m_InfoFilter;
                }
                set
                {
                    m_InfoFilter = value;
                }
            }

            public bool WarningFilter
            {
                get
                {
                    return m_WarningFilter;
                }
                set
                {
                    m_WarningFilter = value;
                }
            }

            public bool ErrorFilter
            {
                get
                {
                    return m_ErrorFilter;
                }
                set
                {
                    m_ErrorFilter = value;
                }
            }

            public bool FatalFilter
            {
                get
                {
                    return m_FatalFilter;
                }
                set
                {
                    m_FatalFilter = value;
                }
            }

            public int InfoCount
            {
                get
                {
                    return m_InfoCount;
                }
            }

            public int WarningCount
            {
                get
                {
                    return m_WarningCount;
                }
            }

            public int ErrorCount
            {
                get
                {
                    return m_ErrorCount;
                }
            }

            public int FatalCount
            {
                get
                {
                    return m_FatalCount;
                }
            }

            public int DebugCount
            {
                get
                {
                    return m_DebugCount;
                }
            }
            public Color32 InfoColor
            {
                get
                {
                    return m_InfoColor;
                }
                set
                {
                    m_InfoColor = value;
                }
            }

            public Color32 WarningColor
            {
                get
                {
                    return m_WarningColor;
                }
                set
                {
                    m_WarningColor = value;
                }
            }

            public Color32 ErrorColor
            {
                get
                {
                    return m_ErrorColor;
                }
                set
                {
                    m_ErrorColor = value;
                }
            }

            public Color32 FatalColor
            {
                get
                {
                    return m_FatalColor;
                }
                set
                {
                    m_FatalColor = value;
                }
            }

            public void Initialize(params object[] args)
            {
                m_SettingComponent = GameService.Setting;
                if (m_SettingComponent == null)
                {
                    Log.Fatal("Setting component is invalid.");
                    return;
                }
                if(m_ReceiveFromUnity)
                    Application.logMessageReceived += OnLogMessageReceived;
                if (m_ReceiveFromCustomLog)
                {
                    Log.OnLogDataReceived += data => AddLogData(data);   
                    for (int i = 0; i < Log.LogDatasCount; i++)
                    {
                        AddLogData(Log.GetLogDataAt(i));
                    }
                    
                }
                    
                
                m_LockScroll = m_LastLockScroll = m_SettingComponent.GetBool("Debugger.Console.LockScroll", true);
                m_InfoFilter = m_LastInfoFilter = m_SettingComponent.GetBool("Debugger.Console.InfoFilter", true);
                m_WarningFilter = m_LastWarningFilter = m_SettingComponent.GetBool("Debugger.Console.WarningFilter", true);
                m_ErrorFilter = m_LastErrorFilter = m_SettingComponent.GetBool("Debugger.Console.ErrorFilter", true);
                m_FatalFilter = m_LastFatalFilter = m_SettingComponent.GetBool("Debugger.Console.FatalFilter", true);
            }

            public void Shutdown()
            {
                Application.logMessageReceived -= OnLogMessageReceived;
                Clear();
            }

            public void OnEnter()
            {
            }

            public void OnLeave()
            {
            }

            public void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                if (m_LastLockScroll != m_LockScroll)
                {
                    m_LastLockScroll = m_LockScroll;
                    m_SettingComponent.SetBool("Debugger.Console.LockScroll", m_LockScroll);
                }

                if (m_LastInfoFilter != m_InfoFilter)
                {
                    m_LastInfoFilter = m_InfoFilter;
                    m_SettingComponent.SetBool("Debugger.Console.InfoFilter", m_InfoFilter);
                }

                if (m_LastWarningFilter != m_WarningFilter)
                {
                    m_LastWarningFilter = m_WarningFilter;
                    m_SettingComponent.SetBool("Debugger.Console.WarningFilter", m_WarningFilter);
                }

                if (m_LastErrorFilter != m_ErrorFilter)
                {
                    m_LastErrorFilter = m_ErrorFilter;
                    m_SettingComponent.SetBool("Debugger.Console.ErrorFilter", m_ErrorFilter);
                }

                if (m_LastFatalFilter != m_FatalFilter)
                {
                    m_LastFatalFilter = m_FatalFilter;
                    m_SettingComponent.SetBool("Debugger.Console.FatalFilter", m_FatalFilter);
                }
            }

            public void OnDraw()
            {
                RefreshCount();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Clear All", GUILayout.Width(100f)))
                    {
                        Clear();
                    }
                    m_LockScroll = GUILayout.Toggle(m_LockScroll, "Lock Scroll", GUILayout.Width(90f));
                    GUILayout.FlexibleSpace();
                    m_DebugFilter = GUILayout.Toggle(m_DebugFilter, string.Format("Debug ({0})", m_DebugCount.ToString()), GUILayout.Width(90f));
                    m_InfoFilter = GUILayout.Toggle(m_InfoFilter, string.Format("Info ({0})", m_InfoCount.ToString()), GUILayout.Width(90f));
                    m_WarningFilter = GUILayout.Toggle(m_WarningFilter, string.Format("Warning ({0})", m_WarningCount.ToString()), GUILayout.Width(90f));
                    m_ErrorFilter = GUILayout.Toggle(m_ErrorFilter, string.Format("Error ({0})", m_ErrorCount.ToString()), GUILayout.Width(90f));
                    m_FatalFilter = GUILayout.Toggle(m_FatalFilter, string.Format("Fatal ({0})", m_FatalCount.ToString()), GUILayout.Width(90f));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                {
                    if (m_LockScroll)
                    {
                        m_LogScrollPosition.y = float.MaxValue;
                    }

                    m_LogScrollPosition = GUILayout.BeginScrollView(m_LogScrollPosition);
                    {
                        bool selected = false;
                        foreach (Log.LogData logNode in m_LogNodes)
                        {
                            switch (logNode.logLevel)
                            {
                                case LogLevel.DEBUG:
                                    if (!m_DebugFilter)
                                    {
                                        continue;
                                    }
                                    break;
                                case LogLevel.INFO:
                                    if (!m_InfoFilter)
                                    {
                                        continue;
                                    }
                                    break;

                                case LogLevel.WARN:
                                    if (!m_WarningFilter)
                                    {
                                        continue;
                                    }
                                    break;

                                case LogLevel.ERROR:
                                    if (!m_ErrorFilter)
                                    {
                                        continue;
                                    }
                                    break;

                                case LogLevel.FATAL:
                                    if (!m_FatalFilter)
                                    {
                                        continue;
                                    }
                                    break;
                            }
                            if (GUILayout.Toggle(m_SelectedNode ==logNode, GetLogString(logNode)))
                            {
                                selected = true;
                                if (m_SelectedNode!=logNode)
                                {
                                    m_SelectedNode = logNode;
                                    m_StackScrollPosition = Vector2.zero;
                                }
                            }
                        }
                        if (!selected)
                        {
                            m_SelectedNode = default;
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                {
                    m_StackScrollPosition = GUILayout.BeginScrollView(m_StackScrollPosition, GUILayout.Height(100f));
                    {
                        if (m_SelectedNode!= null)
                        {
                            Color32 color = GetLogStringColor(m_SelectedNode.logLevel);
                            if (GUILayout.Button(string.Format("<color=#{0}{1}{2}{3}><b>{4}</b></color>{6}{6}{5}", color.r.ToString("x2"), color.g.ToString("x2"), color.b.ToString("x2"), color.a.ToString("x2"), m_SelectedNode.logMessageObject, m_SelectedNode.logTrack, Environment.NewLine), "label"))
                            {
                                CopyToClipboard(string.Format("{0}{2}{2}{1}", m_SelectedNode.logMessageObject, m_SelectedNode.logTrack, Environment.NewLine));
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }

            private void Clear()
            {
                m_LogNodes.Clear();
            }

            public void RefreshCount()
            {
                m_DebugCount = 0;
                m_InfoCount = 0;
                m_WarningCount = 0;
                m_ErrorCount = 0;
                m_FatalCount = 0;
                foreach (Log.LogData logNode in m_LogNodes)
                {
                    switch (logNode.logLevel)
                    {
                        case LogLevel.DEBUG:
                            m_DebugCount++;
                            break;
                        case LogLevel.INFO:
                            m_InfoCount++;
                            break;
                        case LogLevel.WARN:
                            m_WarningCount++;
                            break;
                        case LogLevel.ERROR:
                            m_ErrorCount++;
                            break;

                        case LogLevel.FATAL:
                            m_FatalCount++;
                            break;
                    }
                }
            }

            public void GetRecentLogs(List<Log.LogData> results)
            {
                if (results == null)
                {
                    Log.Error("Results is invalid.");
                    return;
                }

                results.Clear();
                foreach (Log.LogData logNode in m_LogNodes)
                {
                    results.Add(logNode);
                }
            }

            public void GetRecentLogs(List<Log.LogData> results, int count)
            {
                if (results == null)
                {
                    Log.Error("Results is invalid.");
                    return;
                }

                if (count <= 0)
                {
                    Log.Error("Count is invalid.");
                    return;
                }

                int position = m_LogNodes.Count - count;
                if (position < 0)
                {
                    position = 0;
                }

                int index = 0;
                results.Clear();
                foreach (Log.LogData logNode in m_LogNodes)
                {
                    if (index++ < position)
                    {
                        continue;
                    }

                    results.Add(logNode);
                }
            }

            public void AddLogData(Log.LogData logData)
            {
                if( ReferenceEquals(logData ,null)) return;
                m_LogNodes.Enqueue(logData);
                while (m_LogNodes.Count > m_MaxLine)
                {
                    m_LogNodes.Dequeue();
                }
            }

         
            private void OnLogMessageReceived(string logMessage, string stackTrace, LogType logType)
            {
                if (logType == LogType.Assert)
                {
                    logType = LogType.Error;
                }

                AddLogData(Log.LogData.CreateFromUnityLog(logMessage, stackTrace, logType));
            }

            private string GetLogString(Log.LogData logNode)
            {
                Color32 color = GetLogStringColor(logNode.logLevel);
                //return (string) logNode.logMessageObject;
                return string.Format("<color=#{0}{1}{2}{3}>[{4}]{5} {6}</color>",
                    color.r.ToString("x2"), color.g.ToString("x2"), color.b.ToString("x2"), color.a.ToString("x2"),
                    logNode.logTime, logNode.logPrefix,logNode.logMessageObject);
            }

            internal Color32 GetLogStringColor(LogLevel logType)
            {
                Color32 color = Color.white;
                switch (logType)
                {
                    case LogLevel.DEBUG:
                        color = m_DebugColor;
                        break;
                    case LogLevel.INFO:
                        color = m_InfoColor;
                        break;

                    case LogLevel.WARN:
                        color = m_WarningColor;
                        break;

                    case LogLevel.ERROR:
                        color = m_ErrorColor;
                        break;

                    case LogLevel.FATAL:
                        color = m_FatalColor;
                        break;
                }

                return color;
            }
        }
    }
}