using System.Collections.Generic;


namespace UnitFramework.Runtime
{
    [AutoRegisterHelper]
    public class DebuggerManager : IDebuggerManager
    {
      private readonly DebuggerManager.DebuggerWindowGroup m_DebuggerWindowRoot;
      private bool m_ActiveWindow;

      /// <summary>初始化调试器管理器的新实例。</summary>
      public DebuggerManager()
      {
        this.m_DebuggerWindowRoot = new DebuggerManager.DebuggerWindowGroup();
        this.m_ActiveWindow = false;
      }

 

      /// <summary>获取或设置调试器窗口是否激活。</summary>
      public bool ActiveWindow
      {
        get
        {
          return this.m_ActiveWindow;
        }
        set
        {
          this.m_ActiveWindow = value;
        }
      }

      /// <summary>调试器窗口根结点。</summary>
      public IDebuggerWindowGroup DebuggerWindowRoot
      {
        get
        {
          return (IDebuggerWindowGroup) this.m_DebuggerWindowRoot;
        }
      }

      /// <summary>调试器管理器轮询。</summary>
      /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
      /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
      public  void Update(float elapseSeconds, float realElapseSeconds)
      {
        if (!this.m_ActiveWindow)
          return;
        this.m_DebuggerWindowRoot.OnUpdate(elapseSeconds, realElapseSeconds);
      }

     

      /// <summary>关闭并清理调试器管理器。</summary>
      public  void Shutdown()
      {
        this.m_ActiveWindow = false;
        this.m_DebuggerWindowRoot.Shutdown();
      }

      /// <summary>注册调试器窗口。</summary>
      /// <param name="path">调试器窗口路径。</param>
      /// <param name="debuggerWindow">要注册的调试器窗口。</param>
      /// <param name="args">初始化调试器窗口参数。</param>
      public void RegisterDebuggerWindow(
        string path,
        IDebuggerWindow debuggerWindow,
        params object[] args)
      {
        if (string.IsNullOrEmpty(path))
          throw new UnitFrameworkException("Path is invalid.");
        if (debuggerWindow == null)
          throw new UnitFrameworkException("Debugger window is invalid.");
        this.m_DebuggerWindowRoot.RegisterDebuggerWindow(path, debuggerWindow);
        debuggerWindow.Initialize(args);
      }

      /// <summary>解除注册调试器窗口。</summary>
      /// <param name="path">调试器窗口路径。</param>
      /// <returns>是否解除注册调试器窗口成功。</returns>
      public bool UnregisterDebuggerWindow(string path)
      {
        return this.m_DebuggerWindowRoot.UnregisterDebuggerWindow(path);
      }

      /// <summary>获取调试器窗口。</summary>
      /// <param name="path">调试器窗口路径。</param>
      /// <returns>要获取的调试器窗口。</returns>
      public IDebuggerWindow GetDebuggerWindow(string path)
      {
        return this.m_DebuggerWindowRoot.GetDebuggerWindow(path);
      }

      /// <summary>选中调试器窗口。</summary>
      /// <param name="path">调试器窗口路径。</param>
      /// <returns>是否成功选中调试器窗口。</returns>
      public bool SelectDebuggerWindow(string path)
      {
        return this.m_DebuggerWindowRoot.SelectDebuggerWindow(path);
      }

      /// <summary>调试器窗口组。</summary>
      private sealed class DebuggerWindowGroup : IDebuggerWindowGroup, IDebuggerWindow
      {
        private readonly List<KeyValuePair<string, IDebuggerWindow>> m_DebuggerWindows;
        private int m_SelectedIndex;
        private string[] m_DebuggerWindowNames;

        public DebuggerWindowGroup()
        {
          this.m_DebuggerWindows = new List<KeyValuePair<string, IDebuggerWindow>>();
          this.m_SelectedIndex = 0;
          this.m_DebuggerWindowNames = (string[]) null;
        }

        /// <summary>获取调试器窗口数量。</summary>
        public int DebuggerWindowCount
        {
          get
          {
            return this.m_DebuggerWindows.Count;
          }
        }

        /// <summary>获取或设置当前选中的调试器窗口索引。</summary>
        public int SelectedIndex
        {
          get
          {
            return this.m_SelectedIndex;
          }
          set
          {
            this.m_SelectedIndex = value;
          }
        }

        /// <summary>获取当前选中的调试器窗口。</summary>
        public IDebuggerWindow SelectedWindow
        {
          get
          {
            return this.m_SelectedIndex >= this.m_DebuggerWindows.Count ? (IDebuggerWindow) null : this.m_DebuggerWindows[this.m_SelectedIndex].Value;
          }
        }

        /// <summary>初始化调试组。</summary>
        /// <param name="args">初始化调试组参数。</param>
        public void Initialize(params object[] args)
        {
        }

        /// <summary>关闭调试组。</summary>
        public void Shutdown()
        {
          foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in this.m_DebuggerWindows)
            debuggerWindow.Value.Shutdown();
          this.m_DebuggerWindows.Clear();
        }

        /// <summary>进入调试器窗口。</summary>
        public void OnEnter()
        {
          this.SelectedWindow.OnEnter();
        }

        /// <summary>离开调试器窗口。</summary>
        public void OnLeave()
        {
          this.SelectedWindow.OnLeave();
        }

        /// <summary>调试组轮询。</summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
          this.SelectedWindow.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        /// <summary>调试器窗口绘制。</summary>
        public void OnDraw()
        {
        }

        private void RefreshDebuggerWindowNames()
        {
          int num = 0;
          this.m_DebuggerWindowNames = new string[this.m_DebuggerWindows.Count];
          foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in this.m_DebuggerWindows)
            this.m_DebuggerWindowNames[num++] = debuggerWindow.Key;
        }

        /// <summary>获取调试组的调试器窗口名称集合。</summary>
        public string[] GetDebuggerWindowNames()
        {
          return this.m_DebuggerWindowNames;
        }

        /// <summary>获取调试器窗口。</summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>要获取的调试器窗口。</returns>
        public IDebuggerWindow GetDebuggerWindow(string path)
        {
          if (string.IsNullOrEmpty(path))
            return (IDebuggerWindow) null;
          int length = path.IndexOf('/');
          if (length < 0 || length >= path.Length - 1)
            return this.InternalGetDebuggerWindow(path);
          return ((DebuggerManager.DebuggerWindowGroup) this.InternalGetDebuggerWindow(path.Substring(0, length)))?.GetDebuggerWindow(path.Substring(length + 1));
        }

        /// <summary>选中调试器窗口。</summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否成功选中调试器窗口。</returns>
        public bool SelectDebuggerWindow(string path)
        {
          if (string.IsNullOrEmpty(path))
            return false;
          int length = path.IndexOf('/');
          if (length < 0 || length >= path.Length - 1)
            return this.InternalSelectDebuggerWindow(path);
          string name = path.Substring(0, length);
          string path1 = path.Substring(length + 1);
          DebuggerManager.DebuggerWindowGroup debuggerWindow = (DebuggerManager.DebuggerWindowGroup) this.InternalGetDebuggerWindow(name);
          return debuggerWindow != null && this.InternalSelectDebuggerWindow(name) && debuggerWindow.SelectDebuggerWindow(path1);
        }

        /// <summary>注册调试器窗口。</summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <param name="debuggerWindow">要注册的调试器窗口。</param>
        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow)
        {
          if (string.IsNullOrEmpty(path))
            throw new UnitFrameworkException();
          int length = path.IndexOf('/');
          if (length < 0 || length >= path.Length - 1)
          {
            if (this.InternalGetDebuggerWindow(path) != null)
              throw new UnitFrameworkException("Debugger window has been registered.");
            this.m_DebuggerWindows.Add(new KeyValuePair<string, IDebuggerWindow>(path, debuggerWindow));
            this.RefreshDebuggerWindowNames();
          }
          else
          {
            string str = path.Substring(0, length);
            string path1 = path.Substring(length + 1);
            DebuggerManager.DebuggerWindowGroup debuggerWindowGroup = (DebuggerManager.DebuggerWindowGroup) this.InternalGetDebuggerWindow(str);
            if (debuggerWindowGroup == null)
            {
              if (this.InternalGetDebuggerWindow(str) != null)
                throw new UnitFrameworkException("Debugger window has been registered, can not create debugger window group.");
              debuggerWindowGroup = new DebuggerManager.DebuggerWindowGroup();
              this.m_DebuggerWindows.Add(new KeyValuePair<string, IDebuggerWindow>(str, (IDebuggerWindow) debuggerWindowGroup));
              this.RefreshDebuggerWindowNames();
            }
            debuggerWindowGroup.RegisterDebuggerWindow(path1, debuggerWindow);
          }
        }

        /// <summary>解除注册调试器窗口。</summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否解除注册调试器窗口成功。</returns>
        public bool UnregisterDebuggerWindow(string path)
        {
          if (string.IsNullOrEmpty(path))
            return false;
          int length = path.IndexOf('/');
          if (length < 0 || length >= path.Length - 1)
          {
            IDebuggerWindow debuggerWindow = this.InternalGetDebuggerWindow(path);
            int num = this.m_DebuggerWindows.Remove(new KeyValuePair<string, IDebuggerWindow>(path, debuggerWindow)) ? 1 : 0;
            debuggerWindow.Shutdown();
            this.RefreshDebuggerWindowNames();
            return num != 0;
          }
          string name = path.Substring(0, length);
          string path1 = path.Substring(length + 1);
          DebuggerManager.DebuggerWindowGroup debuggerWindow1 = (DebuggerManager.DebuggerWindowGroup) this.InternalGetDebuggerWindow(name);
          return debuggerWindow1 != null && debuggerWindow1.UnregisterDebuggerWindow(path1);
        }

        private IDebuggerWindow InternalGetDebuggerWindow(string name)
        {
          foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in this.m_DebuggerWindows)
          {
            if (debuggerWindow.Key == name)
              return debuggerWindow.Value;
          }
          return (IDebuggerWindow) null;
        }

        private bool InternalSelectDebuggerWindow(string name)
        {
          for (int index = 0; index < this.m_DebuggerWindows.Count; ++index)
          {
            if (this.m_DebuggerWindows[index].Key == name)
            {
              this.m_SelectedIndex = index;
              return true;
            }
          }
          return false;
        }
      }

      public void Dispose()
      {
        
      }
      }
}