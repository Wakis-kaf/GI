
using System;
namespace GFramework.Network
{
    public delegate void RpcCallBack(object resp);

    // 客户端请求消息
    public interface ICaller { }

    // 服务器下发消息
    public interface ICallee { }
}