using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class UnitBehaviourDisableHandler : UnitBehaviourHandler<IUnitDisable>
    {
        public override int Priority => 995;
/*        public override void OnAssignUnitDeRegister(IUnitDisable assignUnit)
        {
            Debug.Log("Unit OnAssignUnitDeRegister ");
            base.OnAssignUnitDeRegister(assignUnit);
            if (assignUnit.Unit.IsUnitEnable)
                assignUnit.OnUnitDisable();
        }*/
        public override void OnAssignUnitDisable(IUnitDisable assignUnit)
        {
            assignUnit.OnUnitDisable();
        }
    }
}
