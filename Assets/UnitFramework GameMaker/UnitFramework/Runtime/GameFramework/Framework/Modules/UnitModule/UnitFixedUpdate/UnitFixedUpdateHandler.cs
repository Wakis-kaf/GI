using System.Collections.Generic;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class UnitBehaviourFixedUpdateHandler: UnitBehaviourHandler<IFixedUpdate>
    {
        private List<IFixedUpdate> mFixedUpdates = new List<IFixedUpdate>(10000);
        public override int Priority => 996;
        
        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            mFixedUpdates.Clear();
            mFixedUpdates = null;
        }
        public override void OnAssignUnitRegister(IFixedUpdate unit)
        {
            base.OnAssignUnitRegister(unit);
            mFixedUpdates.Add(unit);
        }
        public override void OnAssignUnitDeRegister(IFixedUpdate unit)
        {
            base.OnAssignUnitDeRegister(unit);
            mFixedUpdates.Remove(unit);
        }

        public override void OnUnitModuleFixedUpdate()
        {
            base.OnUnitModuleFixedUpdate();
            int count = mFixedUpdates.Count;
            for (int i = 0; i < count; i++)
            {
                IFixedUpdate ctr = mFixedUpdates[i];
                /* IUnit unit = ctr.Unit;
                 if (ReferenceEquals(unit, null) || unit.IsUnitEnable)*/
                ctr.EKFFixedUpdate();
            }
        }

       
      
    }
}