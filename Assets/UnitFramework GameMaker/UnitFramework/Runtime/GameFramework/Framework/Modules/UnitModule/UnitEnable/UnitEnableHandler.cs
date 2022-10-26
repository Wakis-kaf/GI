using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class UnitBehaviourEnableHandler : UnitBehaviourHandler<IUnitEnable>
    {
        /// <summary>
        /// Awake 优先级
        /// </summary>
        public override int Priority => 998;
        /*       private List<IUnitEnable> m_EnableWaitingUnits = new List<IUnitEnable>(1000);
               public override void OnAssignUnitRegister(IUnitEnable assignUnit)
               {
                   base.OnAssignUnitRegister(assignUnit);
                   // 如果已经注册 
                   if (ReferenceEquals(assignUnit.Unit, null) || assignUnit.Unit.IsUnitEnable)
                   {
                       assignUnit.OnUnitEnable();
                   }
                   else
                   {
                       // 加入等待队列
                       m_EnableWaitingUnits.Add(assignUnit);
                   }
               }
               public override void OnAssignUnitDeRegister(IUnitEnable assignUnit)
               {
                   base.OnAssignUnitDeRegister(assignUnit);
                   m_EnableWaitingUnits.Remove(assignUnit);
               }
               public override void OnUnitModuleUpdate()
               {
                   base.OnUnitModuleUpdate();
                   int count = m_EnableWaitingUnits.Count;
                   for (int i = count - 1; i >= 0; i--)
                   {
                       var item = m_EnableWaitingUnits[i];
                       if (item.Unit.IsUnitEnable)
                       {
                           item.OnUnitEnable();
                           // 移除队列
                           m_EnableWaitingUnits[i] = m_EnableWaitingUnits[count - 1];
                           m_EnableWaitingUnits.RemoveAt(count - 1);
                           count -= 1;
                       }
                   }
               }*/
        public override void OnAssignUnitEnable(IUnitEnable assignUnit)
        {
            base.OnAssignUnitEnable(assignUnit);
            assignUnit.OnUnitEnable();
        }
    }
}
