﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public interface IController : IUnitBehaviour
    {
        public string ControllerName { get; }
        public void ControllerUpdate();
    }
}
