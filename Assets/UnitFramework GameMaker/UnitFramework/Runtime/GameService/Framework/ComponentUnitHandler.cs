using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public class UnitComponentBehaviourHandler: UnitBehaviourHandler<IComponentUnit>
    {
        public override void OnAssignUnitRegister(IComponentUnit assignUnit)
        {
            base.OnAssignUnitRegister(assignUnit);
            RegisterHelper(assignUnit);
        }

        private void RegisterHelper(IComponentUnit assignUnit)
        {
            if (assignUnit is IHelperBase extend)
                GameFramework.Container.RegisterHelper(extend);
        }
        public override void OnAssignUnitDeRegister(IComponentUnit assignUnit)
        {
            base.OnAssignUnitDeRegister(assignUnit);
            DeRegisterHelper(assignUnit);
        }

        private void DeRegisterHelper(IComponentUnit assignUnit)
        {
            if (assignUnit is IHelperBase extend)
                // 移除扩展
                GameFramework.Container.RemoveHelper(extend);
        }
    }
}
