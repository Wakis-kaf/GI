using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UnitFramework.Runtime
{
    public class UnitBehaviourAwakeHandler : UnitBehaviourHandler<IUnitAwake>
    {
        /// <summary>
        /// Awake 优先级
        /// </summary>
        public override int Priority => 999;
        private List<IUnitAwake> m_AwakeWaitingUnits = new List<IUnitAwake>(1000);
        public override void OnAssignUnitRegister(IUnitAwake assignUnit)
        {
            base.OnAssignUnitRegister(assignUnit);
            // 如果已经注册 
            if(ReferenceEquals(assignUnit.OwnerUnit,null) || assignUnit.OwnerUnit.IsUnitEnable)
            {
                assignUnit.OnUnitAwake();
            }
            else
            {
                // 加入等待队列
                m_AwakeWaitingUnits.Add(assignUnit);
            }
        }
        public override void OnAssignUnitDeRegister(IUnitAwake assignUnit)
        {
            base.OnAssignUnitDeRegister(assignUnit);
            m_AwakeWaitingUnits.Remove(assignUnit);
        }
        public override void OnUnitModuleUpdate()
        {
            base.OnUnitModuleUpdate();
            int count = m_AwakeWaitingUnits.Count;
            for (int i = count-1; i >= 0; i--)
            {
                var item = m_AwakeWaitingUnits[i];
                if (item.OwnerUnit.IsUnitEnable)
                {
                    item.OnUnitAwake();
                    // 移出队列
                    m_AwakeWaitingUnits[i] = m_AwakeWaitingUnits[count - 1];
                    m_AwakeWaitingUnits.RemoveAt(count - 1);
                    count -= 1;
                }  
            }
        }
        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            m_AwakeWaitingUnits.Clear();
            m_AwakeWaitingUnits = null;
        }
    }
}
