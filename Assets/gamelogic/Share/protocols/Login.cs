using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;

namespace GI.Assets.gamelogic.Share.protocols
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LoginReq
    {
        public string userName;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LoginResp
    {
        public int ok = 1;
    }
}