using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public interface IUnitEnable : IUnitBehaviour
    {
        public void OnUnitEnable();
    }
}
