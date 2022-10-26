using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public interface IUnitBehaviourHandler : IFrameObject
    {
        public int Priority { get; }
        public void OnUnitHandleEventTrigger(IUnit unit, UnitHandleType handleType);
        public void OnUnitModuleQuit();
        public void OnUnitModuleUpdate();
        public void OnUnitModuleFixedUpdate();
    }
}
