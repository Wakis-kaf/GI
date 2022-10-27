using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitFramework.Runtime.Network;
public enum MsgRegisterResult
{
    Success,
    AccountInvalidate,
    AlreadyRegister,
    ServerError
}
public enum MsgLoginResult
{
    Success,
    AccountInvalidate,
    AccountNotExist,
    AlreadyLogin,
    SuccessAndKickOther,
}
//注册
public class MsgRegister : MsgBase
{
    public MsgRegister()
    {
        protoName = "MsgRegister";
    }

    //客户端发
    public string id = "";
    public string phone = "";
    public string mail = "";
    public string pw = "";

    //服务端回（0-成功，1-失败）
    public int result = 0;
}

//登陆
public class MsgLogin : MsgBase
{
    public MsgLogin()
    {
        protoName = "MsgLogin";
    }

    //客户端发
    public string id = "";
    public string phone = "";
    public string mail = "";
    public string pw = "";

    //服务端回（0-成功，1-失败）
    public int result = 0;
}
