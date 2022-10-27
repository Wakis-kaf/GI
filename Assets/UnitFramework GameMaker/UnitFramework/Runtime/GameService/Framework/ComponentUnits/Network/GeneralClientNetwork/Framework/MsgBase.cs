using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitFramework.Runtime.Network
{
    
    public class MsgBase
    {
        // 协议名,在派生类中，protoName必须和派生类的类名相同
        public string protoName;
    
        /// <summary>
        /// 对JSOn协议包进行解析
        /// </summary>
        /// <param name="protoName">协议名</param>
        /// <param name="bytes">二进制流</param>
        /// <param name="offset">解析偏移下标</param>
        /// <param name="count">解析长度</param>
        /// <returns></returns>
        public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
        {
            string s = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
            var type = Type.GetType(protoName);

            if (type == null)
            {
                return null;
            }
            MsgBase msgBase = (MsgBase)JsonUtility.FromJson(s, type);
            return msgBase;
        }
    
        /// <summary>
        /// 对协议体进行编码
        /// </summary>
        /// <param name="msgBase">要编码的协议体</param>
        /// <returns>编码后的二进制数组</returns>
        public static byte[] Encode(MsgBase msgBase)
        {
            string s = JsonUtility.ToJson(msgBase);
            return System.Text.Encoding.UTF8.GetBytes(s);
        }
    
        /// <summary>
        /// 对协议名进行编码(2字节长度 + 字符串)
        /// </summary>
        /// <param name="msgBase"></param>
        /// <returns></returns>
        public static byte[] EncodeName(MsgBase msgBase)
        {
            // 名字bytes和长度
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.protoName);
            Int16 len = (Int16)nameBytes.Length;
            // 申请bytes数组
            byte[] bytes = new byte[2 + len];
            // 组装2字节的长度信息
            //此处采用小端模式存储
    
            bytes[0] = (byte)(len % 256);
            bytes[1] = (byte)(len / 256);
            // 组装名字bytes
            Array.Copy(nameBytes, 0, bytes, 2, len);
            return bytes;
        }
    
        public static string DecodeName(byte[] bytes, int offset, out int count)
        {
            count = 0;
            if (offset + 2 > bytes.Length)
            {
                return "";
            }
    
            // 读取长度
            // 取高位× 2^8,低位× 2^0,高位和低位相加
            Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
            if (len <= 0)
            {
                return "";
            }
            // 长度必须足够
            if (offset + 2 + len > bytes.Length)
            {
                return "";
            }
            // 解析
            count = 2 + len;
            string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
            return name;
        }
    }
}
