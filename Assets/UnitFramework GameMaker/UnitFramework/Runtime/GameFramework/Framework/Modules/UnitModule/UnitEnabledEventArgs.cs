using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public class UnitEnabledEventArgs : FrameEventArgs
    {
        public IUnit unit { get; private set; }
        public bool enable = false;

        public UnitEnabledEventArgs(IUnit unit) {
            this.unit = unit;
        }
        
    }
}
