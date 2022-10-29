using System;
using System.IO;

using ProtoBuf;

namespace GFramework.Network
{
    // ProtocolBuf Net序列化
    public class ProtoBufNetSerializer
    {
        /// <summary>
        /// 序列化对象得到字节
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Encode<T>(T obj)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Serializer.SerializeWithLengthPrefix<T>(ms, obj, ProtoBuf.PrefixStyle.Base128);
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 反序列化字节得到对象
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static T Decode<T>(byte[] data)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    T obj = Serializer.DeserializeWithLengthPrefix<T>(ms, ProtoBuf.PrefixStyle.Base128);
                    return obj;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
