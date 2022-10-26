using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public class UnitBehaviourDestroyHandler : UnitBehaviourHandler<IUnitDestroy>
    {
        public override int Priority => 990;
        public override void OnAssignUnitDeRegister(IUnitDestroy assignUnit)
        {
            base.OnAssignUnitDeRegister(assignUnit);
            assignUnit.OnUnitDestroy();
        }
    }
}
