using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GFramework.Network;

namespace GI.Assets.gamelogic.Network
{
    public interface LogicService : ICaller
    {
        void ToLogin();
    }
}