using System.Collections;
using System.Collections.Generic;
using UnitFramework.Runtime.Network;
using UnityEngine;


public class MsgMove : MsgBase
{
    public MsgMove()
    {
        protoName = "MsgMove";
    }

    public float x;
    public float y;
    public float z;
}