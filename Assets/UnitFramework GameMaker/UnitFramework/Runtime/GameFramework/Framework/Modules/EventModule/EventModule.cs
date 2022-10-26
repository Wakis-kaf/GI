using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitFramework.Utils;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 事件模块
    /// </summary>
    [AutoRegisterModule]
    public class EventModule : Module
    {
        public override int Priority => (int) GameFrameworkConfig.FrameModuleConfig.ModulePriority.EventModule;
        
        private Dictionary<int, Action<FrameEventArgs>> m_EventId2ActionMap = new Dictionary<int, Action<FrameEventArgs>>();
       
        /// <summary>
        /// 创建事件参数
        /// </summary>
        /// <param name="objects"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateEventArgs<T>(params object[] objects)where T : FrameEventArgs
        {
            T args = ReflectionTool.CreateInstance<T>(objects);
            
            return args;
        }
        /// <summary>
        /// 匿名订阅事件
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="action"></param>
        public void Subscribe(int eventId,Action<FrameEventArgs> action)
        {
            if (m_EventId2ActionMap.ContainsKey(eventId))
                m_EventId2ActionMap[eventId] += action;
            else
                m_EventId2ActionMap.Add(eventId, action);
        }
        /// <summary>
        /// 匿名取消订阅事件
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="action"></param>
        public void UnSubscribe(int eventId,Action<FrameEventArgs> action)
        {
            if (m_EventId2ActionMap.ContainsKey(eventId) && m_EventId2ActionMap[eventId] != null)
                m_EventId2ActionMap[eventId] -= action;
        }
        /// <summary>
        /// 匿名取消订阅指定的全部事件
        /// </summary>
        /// <param name="eventId"></param>
        public void UnSubscribe(int eventId)
        {
            m_EventId2ActionMap.Remove(eventId);
        }
        /// <summary>
        /// 匿名发布事件
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="args"></param>
        public void Dispatch(int eventId,FrameEventArgs args = null)
        {
            if(m_EventId2ActionMap.TryGetValue(eventId,out Action<FrameEventArgs> action)){
                if (args != null)
                    args.eventCode = eventId;
                action(args);
            }
        }
    }
}
