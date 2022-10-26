using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnitFramework.Runtime
{
    
    public sealed partial class ArchiveSystem
    {
        /// <summary>
        /// 二进制存档序列化
        /// </summary>
        private class BinaryArchiveSerializer : IArchiveSerializer
        {

            public bool Serialize(Stream stream, object data)
            {
                // 使用二进制进行序列化
                // 将对象进行 序列化封装之后进行保存
                var bf = ObjectSerializer.GetBinaryFormatter(data);
                
                bf.Serialize(stream,data);
                return true;
            }

            public T DeSerialize<T>(Stream stream)
            {
                var bf = ObjectSerializer.GetBinaryFormatter();
                return (T)bf.Deserialize(stream) ;
            }

       
        }
        
    }
    public class ArchiveSerializerException : Exception
    {
        public ArchiveSerializerException(string s) : base(s)
        {
            
        }
    }
}