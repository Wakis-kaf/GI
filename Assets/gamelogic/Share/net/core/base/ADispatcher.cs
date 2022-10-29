using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GFramework.Network;

namespace GFramework.Network
{
    public abstract class ADispatcher
    {
        public AChannel channel = null;
        public abstract void DecodeForm(ProtoDefine define, byte[] data);
        public abstract void RegisterMsg(RpcCallBack response);
    }
}