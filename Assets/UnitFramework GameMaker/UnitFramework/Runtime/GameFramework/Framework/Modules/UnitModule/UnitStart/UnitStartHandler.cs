using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public class UnitBehaviourStartHandler : UnitBehaviourHandler<IUnitStart>
    {
        public override int Priority => 997;
        private List<IUnitStart> m_StartWaitingUnits = new List<IUnitStart>(1000);
        public override void OnAssignUnitRegister(IUnitStart assignUnit)
        {
            base.OnAssignUnitRegister(assignUnit);
            // 加入等待队列
            m_StartWaitingUnits.Add(assignUnit);
        }
        public override void OnAssignUnitDeRegister(IUnitStart assignUnit)
        {
            base.OnAssignUnitDeRegister(assignUnit);
            m_StartWaitingUnits.Remove(assignUnit);
        }
        public override void OnUnitModuleUpdate()
        {
            base.OnUnitModuleUpdate();
            int count = m_StartWaitingUnits.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var item = m_StartWaitingUnits[i];
                if (item.OwnerUnit.IsUnitEnable)
                {
                    item.OnUnitStart();
                    // 移除队列
                    m_StartWaitingUnits[i] = m_StartWaitingUnits[count - 1];
                    m_StartWaitingUnits.RemoveAt(count - 1);
                    count -= 1;
                }
            }
        }
        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            m_StartWaitingUnits.Clear();
            m_StartWaitingUnits = null;
        }
    }
}
