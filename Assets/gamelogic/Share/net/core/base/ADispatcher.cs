using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UGFramework.Network;

namespace UGFramework.Network
{
    public abstract class ADispatcher
    {
        public AChannel channel = null;
        public abstract void DecodeForm(ProtoDefine define, byte[] data);
        public abstract void RegisterMsg(RpcCallBack response);
    }
}