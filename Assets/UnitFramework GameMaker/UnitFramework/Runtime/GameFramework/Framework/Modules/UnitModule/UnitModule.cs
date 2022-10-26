using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitFramework.Utils;
using UnityEngine;

namespace UnitFramework.Runtime
{
    [AutoRegisterModule]
    public class UnitModule : Module
    {
        public override int Priority => (int) GameFrameworkConfig.FrameModuleConfig.ModulePriority.UnitModule;
        private List<IUnit> m_Units;
        private List<IUnitBehaviourHandler> m_UnitHandlers;
        private event Action m_UnitModuleQuit;
        private event Action<IUnit, UnitHandleType> m_UnitHandleEvents;
        
        public UnitModule()
        {
            m_Units = new List<IUnit>(10000);
            m_UnitHandlers = new List<IUnitBehaviourHandler>(64);
            UnitHandlerInit();
        }
        private void UnitHandlerInit()
        {
            // 获取所有实现了 UnitHandler接口的类并创建其实例
            Type[] unitHandlers = ReflectionTool.GetSubClassOfRawGeneric(typeof(UnitBehaviourHandler<>));

            RegisterUnitHandlers(unitHandlers);
        }

        private void RegisterUnitHandlers(Type[] unitHandlerTypes)
        {
            for (int i = 0; i < unitHandlerTypes.Length; i++)
            {
                var handlerType = unitHandlerTypes[i];
                if (handlerType.IsGenericType) continue;
                var handler = ReflectionTool.CreateInstance<IUnitBehaviourHandler>(handlerType);
                m_UnitHandlers.Add(handler);
            }
            m_UnitHandlers.Sort(HandlerCompare);

            BindHandleEvent();
        }
        /// <summary>按照优先等级降序排序处理器/// </summary>
        private int HandlerCompare(IUnitBehaviourHandler one, IUnitBehaviourHandler two)
        {
            if (one.Priority < two.Priority)
                return 1;
            return -1;
        }

        private void BindHandleEvent()
        {
            int length = m_UnitHandlers.Count;
            for (int i = 0; i < length; i++)
            {
                var handler = m_UnitHandlers[i];
                // 绑定事件
                m_UnitModuleQuit += handler.OnUnitModuleQuit;
                m_UnitHandleEvents += handler.OnUnitHandleEventTrigger;
            }
        }

        public override void OnFrameStart()
        {
            base.OnFrameStart();
            // 绑定事件
            GameFramework.Event.Subscribe((int) UnitHandleType.UnitEnable, OuUnitEnable);
            GameFramework.Event.Subscribe((int) UnitHandleType.UnitDisable, OnUnitDisable);
        }
        private void OuUnitEnable(FrameEventArgs args)
        {
            // 触发事件
            UnitEnabledEventArgs enabledEventArgs = args as UnitEnabledEventArgs;
            m_UnitHandleEvents?.Invoke(enabledEventArgs.unit, UnitHandleType.UnitEnable);
        }
        private void OnUnitDisable(FrameEventArgs args)
        {
            // 触发事件
            UnitEnabledEventArgs enabledEventArgs = args as UnitEnabledEventArgs;
            m_UnitHandleEvents?.Invoke(enabledEventArgs.unit, UnitHandleType.UnitDisable);
        }


        public override void OnFrameUpdate()
        {
            base.OnFrameUpdate();
            UpdateUnitHandlers();
        }

        private void UpdateUnitHandlers()
        {
            int count = m_UnitHandlers.Count;
            for (int i = 0; i < count; i++)
            {
                var item = m_UnitHandlers[i];
                item.OnUnitModuleUpdate();
            }
        }

        public override void OnFrameFixedUpdate()
        {
            base.OnFrameFixedUpdate();
            FixedUpdateUnitHandlers();
        }

        private void FixedUpdateUnitHandlers()
        {
            int count = m_UnitHandlers.Count;
            for (int i = 0; i < count; i++)
            {
                var item = m_UnitHandlers[i];
                item.OnUnitModuleFixedUpdate();
            }
        }

        public override void OnFrameworkShutdown()
        {
            base.OnFrameworkShutdown();
            m_UnitModuleQuit?.Invoke();
        }
        
        public T RegisterUnit<T>(T unit) where T : IUnit
        {
            if (ReferenceEquals(unit, null)) return default(T);
            // 加入队列
            //if (m_Units.Contains(unit)) return unit;
            m_Units.Add(unit);
            // 触发事件
            m_UnitHandleEvents?.Invoke(unit, UnitHandleType.UnitRegister);
            return unit;
        }

        public void DeRegisterUnit(IUnit unit)
        {
            // 移出队列
            int index = m_Units.IndexOf(unit);
            if (index == -1) return;
            // 触发事件
            m_UnitHandleEvents?.Invoke(unit, UnitHandleType.UnitDeRegister);
            int lastIndex = m_Units.Count - 1;
            m_Units[index] = m_Units[lastIndex];
            m_Units.RemoveAt(lastIndex);
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            int length = m_Units.Count;
            for (int i = length - 1; i >= 0; i--)
            {
                if (i >= m_Units.Count) continue;
                m_Units[i].Dispose();
            }

            length = m_UnitHandlers.Count;
            for (int i = length - 1; i >= 0; i--)
            {
                m_UnitHandlers[i].Dispose();
            }
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            m_Units.Clear();
            m_UnitHandlers.Clear();
            m_Units = null;
            m_UnitHandlers = null;
            m_UnitHandleEvents = null;
            m_UnitModuleQuit = null;
            // 移除事件
            GameFramework.Event.UnSubscribe((int) UnitHandleType.UnitEnable);
            GameFramework.Event.UnSubscribe((int) UnitHandleType.UnitDisable);
        }
    }
}