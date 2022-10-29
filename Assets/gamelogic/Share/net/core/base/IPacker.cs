using System;
using System.Collections.Generic;

namespace GFramework.Network
{
    public interface IPacker
    {
        byte[] Pack(ProtoDefine protoDefine, byte[] data);
        List<Tuple<ProtoDefine, byte[]>> UnPack(ref byte[] buffer, ref int bufferSize);
    }
}