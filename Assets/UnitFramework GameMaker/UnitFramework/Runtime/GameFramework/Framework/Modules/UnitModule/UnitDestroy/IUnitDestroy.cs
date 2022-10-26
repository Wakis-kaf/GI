using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UnitFramework.Runtime
{
    public interface IUnitDestroy : IUnitBehaviour
    {
        public void OnUnitDestroy();
    }
}
