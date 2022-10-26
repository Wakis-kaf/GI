using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{

    public interface IFrameObject : IDisposable
    {
        public Type Type { get; }
        public bool Disposed { get; }
    }
}