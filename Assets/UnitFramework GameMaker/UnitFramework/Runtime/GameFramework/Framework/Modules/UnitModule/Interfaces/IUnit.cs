using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public interface IUnit : IFrameObject
    {
        public string UnitName { get;}
        public int UnitPriority { get; } // 单元优先级
        public bool IsUnitEnable { get; }
        public IUnit Parent { get; }
        public IUnit OwnerUnit { get; }
        public IUnit SetParent(IUnit parent);
        public int ChildCount { get; }
        public bool TryGetUnit<T>(out T unit) where T : IUnit;
        public bool TryGetUnits<T>(out T[] units) where T : IUnit;
        public T AddChildUnit<T>(T unit) where T : IUnit;
        public T GetUnit<T>() where T : IUnit;

        public int ChildIndexOf(IUnit unit);
        
        public bool HasChild(IUnit child);
        public void RemoveChildUnit<T>() where T : IUnit;
        public void RemoveChildUnit(IUnit unit);
        public void RemoveChildUnitAt(int index);
        public void UnitEnable();
        public void UnitDisable();
    }
    
}
