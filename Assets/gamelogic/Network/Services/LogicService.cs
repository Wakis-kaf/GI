using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UGFramework.Network;

namespace GI.Assets.gamelogic.Network
{
    public interface LogicService : ICaller
    {
        void ToLogin();
    }
}