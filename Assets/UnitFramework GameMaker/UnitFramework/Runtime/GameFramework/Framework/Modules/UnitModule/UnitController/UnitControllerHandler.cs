using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class UnitBehaviourControllerHandler : UnitBehaviourHandler<IController>
    {
        public override int Priority => 996;
        private List<IController> m_Controllers = new List<IController>(10000);
        public override void OnAssignUnitRegister(IController unit)
        {
            base.OnAssignUnitRegister(unit);
            m_Controllers.Add(unit);
        }
        public override void OnAssignUnitDeRegister(IController unit)
        {
            base.OnAssignUnitDeRegister(unit);
            m_Controllers.Remove(unit);

        }
        public override void OnUnitModuleUpdate()
        {
           
            base.OnUnitModuleUpdate();
            int count = m_Controllers.Count;
            for (int i = 0; i < count; i++)
            {
                IController ctr = m_Controllers[i];
                IUnit unit = ctr.OwnerUnit;
                if (ReferenceEquals(unit, null) || !unit.IsUnitEnable) continue;
              
                ctr.ControllerUpdate();
            }
        }
        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            m_Controllers.Clear();
            m_Controllers = null;
        }

    }
}
