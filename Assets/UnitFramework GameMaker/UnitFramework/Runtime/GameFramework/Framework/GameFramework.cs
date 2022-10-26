using System;
using System.Collections.Generic;
using UnitFramework.Utils;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public sealed partial class GameFramework
    {
        /// <summary>数据模型模块 </summary>
        public static ModelModule Model => Instance.GetModule<ModelModule>();

        /// <summary>Unit 模块</summary>
        public static UnitModule Unit => Instance.GetModule<UnitModule>();

        /// <summary>容器模块</summary>
        public static ContainerModule Container => Instance.GetModule<ContainerModule>();

        /// <summary>事件模块</summary>
        public static EventModule Event => Instance.GetModule<EventModule>();
    }

    /// <summary>
    /// 游戏框架
    /// </summary>
    public sealed partial class GameFramework : SingletonObject<GameFramework>
    {
        /// <summary>
        /// 当前框架是否处于运行状态
        /// </summary>
        public bool IsPlaying { get; private set; } = false;

        /// <summary>
        ///  当前框架的所有模块 
        /// </summary>
        private List<Module> m_FrameModules = new List<Module>(10);

        /// <summary>
        /// 模块类型到模块的映射
        /// </summary>
        private Dictionary<Type, Module> mType2ModuleMap = new Dictionary<Type, Module>();

        public GameFramework()
        {
            // 读取模块,创建模块
            FrameModuleInit();
        }

        private void FrameModuleInit()
        {
            Log.DebugInfo("Frame Module Registering ...");
            // 获取所有有Module标签的模块
            Type[] moduleTypes = ReflectionTool.GetSubClassOf<Module>(typeof(AutoRegisterModuleAttribute));
            moduleTypes = ModuleTypeAopProxyCheck(moduleTypes);
            // 添加模块的实例
            ModuleCreateAndRegister(moduleTypes);
        }

        private Type[] ModuleTypeAopProxyCheck(Type[] moduleTypes)
        {
            // 对Module 模块检测是否含有可代理属性，如果可代理就创建
            for (int i = 0; i < moduleTypes.Length; i++)
            {
                //Debug.Log($"检测模块是否是可以代理类型{moduleTypes[i]}");
                var moduleType = moduleTypes[i];
                bool isFindProxy = ProxyBuilder.TryBuildProxyType(moduleType, out Type proxyType);
                if (isFindProxy)
                    Log.DebugInfoFormat("Find Proxy Module {0}", proxyType);
                moduleTypes[i] = proxyType;
            }

            return moduleTypes;
        }

        private void ModuleCreateAndRegister(Type[] moduleTypes)
        {
            for (int i = 0; i < moduleTypes.Length; i++)
            {
                Log.DebugInfo("Module Register..." + moduleTypes[i]);
                AddModule(ReflectionTool.CreateInstance<Module>(moduleTypes[i]));
            }
        }

        public T AddModule<T>(T module) where T : Module
        {
            if (m_FrameModules.Contains(module)) return module;
            m_FrameModules.Add(module);
            m_FrameModules.Sort(ModuleCompare);
            mType2ModuleMap.Add(module.Type, module);
            return module;
        }

        private int ModuleCompare(Module m1, Module m2)
        {
            return m1.Priority > m2.Priority ? 1 : -1;
        }

        /// <summary>
        /// 框架启动
        /// </summary>
        public void StartFramework()
        {
            Log.DebugInfo("Frame Starting...");
            try
            {
                IsPlaying = true;
                StartFrameModules();
            }
            catch (Exception e)
            {
                Log.FatalFormat("Frame Exception In Frame Update {0}!", e.Message);
            }
        }

        private void StartFrameModules()
        {
            int count = m_FrameModules.Count;
            for (int i = 0; i < count; i++)
            {
                m_FrameModules[i].OnFrameStart();
            }
        }

        /// <summary>
        /// 框架轮询更新
        /// </summary>
        public void OnFrameUpdate()
        {
            try
            {
                UpdateFrameModules();
            }
            catch (Exception e)
            {
                Log.FatalFormat("Frame Exception In Frame Update {0}!", e.Message);
            }
        }

        private void UpdateFrameModules()
        {
            int count = m_FrameModules.Count;
            for (int i = 0; i < count; i++)
            {
                m_FrameModules[i].OnFrameUpdate();
            }
        }

        /// <summary>
        /// 框架固定轮询
        /// </summary>
        public void OnFrameFixedUpdate()
        {
            try
            {
                FixedUpdateFrameModules();
            }
            catch (Exception e)
            {
                Log.FatalFormat("Frame Exception In Frame FixedUpdate !", e.Message);
            }
        }

        private void FixedUpdateFrameModules()
        {
            int count = m_FrameModules.Count;
            for (int i = 0; i < count; i++)
            {
                m_FrameModules[i].OnFrameFixedUpdate();
            }
        }

        /// <summary>
        /// 框架退出
        /// </summary>
        public void ShutdownFramework()
        {
            Log.DebugInfo("Frame Quiting...");
            IsPlaying = false;
            ShutdownFrameModules();
            Dispose(); // 回收框架的内存
            ProxyCollections.Clear(); // 清楚代理收集器的资源
        }

        private void ShutdownFrameModules()
        {
            int count = m_FrameModules.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                m_FrameModules[i].OnFrameworkShutdown(); // 调用模块的退出回调
                m_FrameModules[i].Dispose(); // 回收模块的内存
            }
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            m_FrameModules.Clear();
            mType2ModuleMap.Clear();
        }

        /// <summary>
        /// 获取module
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetModule<T>() where T : Module
        {
            Type type = typeof(T);
            
            if (mType2ModuleMap.ContainsKey(type))
            {
                return (T)mType2ModuleMap[type];
            }
            
            int count = m_FrameModules.Count;
            for (int i = 0; i < count; i++)
            {
                if (m_FrameModules[i] is T res)
                {
                    mType2ModuleMap.Add(type, res);
                    return res;
                }
            }

            return default;
        }

        /// <summary>
        /// 移除模块
        /// </summary>
        /// <param name="module"></param>
        public void RemoveModule(Module module)
        {
            if (!m_FrameModules.Contains(module)) return;
            m_FrameModules.Remove(module);
            mType2ModuleMap.Remove(module.GetType());
        }
    }
}