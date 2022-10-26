using System;
using System.Collections.Generic;
using System.Linq;
using UnitFramework.Utils;
using UnityEngine;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 扩展组件
    /// </summary>
    [AutoRegisterModule]
    public class ContainerModule : Module
    {
        public override int Priority => (int) GameFrameworkConfig.FrameModuleConfig.ModulePriority.ExtensionModule;
        private List<IHelperBase> m_Helpers = new List<IHelperBase>();
        private Dictionary<Type, object> m_Type2HelperArrayMap = new Dictionary<Type, object>();
        private Dictionary<Type, object> m_Type2HelperListMap = new Dictionary<Type, object>();
        private Dictionary<Type, object> m_type2HelperMap = new Dictionary<Type, object>();

        public ContainerModule()
        {
            HelpersInit();
        }

        private void HelpersInit()
        {
            // 获取类中所有的 extend base
            Type[] helperTypes = ReflectionTool.GetAssignedClassOf<IHelperBase>(typeof(AutoRegisterHelperAttribute));
            RegisterHelper(helperTypes);
        }

        private void RegisterHelper(Type[] helperTypes)
        {
            foreach (var helper in helperTypes)
            {
                if (helper.IsSubclassOf(typeof(MonoBehaviour))) continue;
                RegisterHelper(ReflectionTool.CreateInstance<IHelperBase>(helper));
            }
        }

        public T RegisterHelper<T>(T extend) where T : IHelperBase
        {
            if (m_Helpers.Contains(extend)) return extend;
            Log.DebugInfo($"[Helper Register ....] {extend.GetType()}");
            m_Helpers.Add(extend);
            return extend;
        }

        public override void OnFrameStart()
        {
            base.OnFrameStart();
            MonoHelpersInit();
        }

        private void MonoHelpersInit()
        {
            var monoHelpers = FindAllMonoHelpers();
            RegisterMonoHelpers(monoHelpers);
        }

        private MonoBehaviour[] FindAllMonoHelpers()
        {
            return Transform.FindObjectsOfType<MonoBehaviour>(true).Where((item) =>
            {
                return (item is IHelperBase) &&
                       Attribute.IsDefined(item.GetType(), typeof(AutoRegisterHelperAttribute));
            }).ToArray();
        }

        private void RegisterMonoHelpers(MonoBehaviour[] monoHelpers)
        {
            foreach (var monoHelper in monoHelpers)
            {
                RegisterHelper(monoHelper as IHelperBase);
            }
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            for (int i = m_Helpers.Count - 1; i >= 0; i--)
            {
                m_Helpers[i].Dispose();
            }
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            m_Helpers.Clear();
            m_type2HelperMap.Clear();
            m_Type2HelperArrayMap.Clear();
            m_Type2HelperListMap.Clear();
        }

        public void RemoveHelper<T>(T helper) where T : IHelperBase
        {
            int index = m_Helpers.IndexOf(helper);
            if (index == -1) return;
            var type = typeof(T);
            if (m_type2HelperMap.TryGetValue(type, out object obj))
            {
                m_type2HelperMap.Remove(type);
            }

            if (m_Type2HelperArrayMap.TryGetValue(type, out object objs))
            {
                List<T> helperList = m_Type2HelperListMap[type] as List<T>;
                helperList.Remove(helper);
                m_Type2HelperArrayMap[type] = helperList.ToArray();
            }

            m_Helpers.RemoveAt(index);
        }

        public T[] GetHelpers<T>(bool isReadOrSaveCache = true) where T : IHelperBase
        {
            int count = m_Helpers.Count;
            Type helperType = typeof(T);
            if (isReadOrSaveCache && HasHelpersCache(helperType))
            {
                return ReadHelpersFromCache<T>(helperType);
            }

            List<T> helperList = new List<T>();
            for (int i = 0; i < count; i++)
            {
                if (m_Helpers[i] is T item)
                {
                    helperList.Add(item);
                }
            }

            T[] helpers = helperList.ToArray();
            if (isReadOrSaveCache && !m_Type2HelperArrayMap.ContainsKey(helperType))
            {
                m_Type2HelperArrayMap.Add(helperType, helpers);
                m_Type2HelperListMap.Add(helperType, helperList);
            }

            return helpers;
        }

        private T[] ReadHelpersFromCache<T>(Type helperType) where T : IHelperBase
        {
            if (m_Type2HelperArrayMap.TryGetValue(helperType, out object genericArrayObject))
            {
                return genericArrayObject as T[];
            }

            return Array.Empty<T>();
        }

        private bool HasHelpersCache(Type helperType)
        {
            return m_Type2HelperArrayMap.ContainsKey(helperType);
        }

        public T FindHelper<T>(bool isFindInOrSaveToCache = true) where T : IHelperBase
        {
            int count = m_Helpers.Count;
            Type helperType = typeof(T);
            if (isFindInOrSaveToCache)
            {
                if (m_type2HelperMap.TryGetValue(helperType, out object obj))
                {
                    return (T) obj;
                }
            }

            for (int i = 0; i < count; i++)
            {
                if (m_Helpers[i] is T item)
                {
                    if (isFindInOrSaveToCache && !m_type2HelperMap.ContainsKey(helperType))
                    {
                        m_type2HelperMap.Add(helperType, item);
                    }

                    return item;
                }
            }

            return default;
        }
    }
}