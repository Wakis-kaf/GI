using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public enum UnitHandleType
    {
        UnitRegister = 101,
        UnitDeRegister = 102,
        UnitEnable =103,
        UnitDisable = 104,
    }
    

    public abstract class UnitBehaviourHandler<T> : FrameObject, IUnitBehaviourHandler
    {
        
        private Dictionary<UnitHandleType, Action<T>> m_handleType2HandleActionMap = new Dictionary<UnitHandleType, Action<T>>();
        public virtual int Priority => 1;
        public UnitBehaviourHandler()
        {
            RegisterInitialHandlers(m_handleType2HandleActionMap);
        }
        protected virtual void RegisterInitialHandlers(Dictionary<UnitHandleType, Action<T>> handlerDict)
        {
            handlerDict.Add(UnitHandleType.UnitRegister, OnAssignUnitRegister);
            handlerDict.Add(UnitHandleType.UnitDeRegister, OnAssignUnitDeRegister);
            handlerDict.Add(UnitHandleType.UnitEnable, OnAssignUnitEnable);
            handlerDict.Add(UnitHandleType.UnitDisable, OnAssignUnitDisable);
        }
     
        public virtual void OnUnitHandleEventTrigger(IUnit unit, UnitHandleType handleType)
        {
            if (m_handleType2HandleActionMap.ContainsKey(handleType) && unit is T tUnitItem)
                m_handleType2HandleActionMap[handleType]?.Invoke(tUnitItem);
        }

        public virtual void OnUnitModuleQuit() { }
        public virtual void OnUnitModuleUpdate()
        {

        }
        public virtual void OnUnitModuleFixedUpdate()
        {
            
        }

        public virtual void OnAssignUnitRegister(T assignUnit) { }
        public virtual void OnAssignUnitDeRegister(T assignUnit) { }
        public virtual void OnAssignUnitEnable(T assignUnit) { }
        public virtual void OnAssignUnitDisable(T assignUnit) { }
      
    }
}
