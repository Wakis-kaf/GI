using System;
using UnitFramework.Utils;
using UnityEngine;

namespace UnitFramework.Runtime
{
    [AutoRegisterHelper]
    public abstract class ComponentUnit : MonoUnit, IComponentUnit, IHelperBase,IUnitAwake
    {
        [SerializeField]
        private  string m_ComponentUnitName;
        [SerializeField]
        private int m_ComponentUnitPriorty = 999;
        
        public override int UnitPriority =>m_ComponentUnitPriorty; 
        public virtual string ComponentUnitName => m_ComponentUnitName;
        

        public ComponentUnit()
        {
            OnValidate();
        }
        public void OnValidate()
        {
            if (ComponentUnitName != m_ComponentUnitName)
            {
                m_ComponentUnitName = ComponentUnitName;
            }

            if (UnitPriority != m_ComponentUnitPriorty)
            {
                m_ComponentUnitPriorty = UnitPriority;
            }
        }

        public virtual void OnUnitAwake()
        {
            ComponentUnitInit();
        }

        private void ComponentUnitInit()
        {
            // 获取所有的组件
            ComponentUnit[] componentUnits = GetAllComponentUnit();
            foreach (var componentUnit in componentUnits)
            {
                AddChildUnit(componentUnit);
            }
        }
        private ComponentUnit[] GetAllComponentUnit()
        {
            return Utility.UnityTransform.GetComponentsInChild<ComponentUnit>(transform);
        }
    }
    
}