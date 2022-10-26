using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitFramework.Runtime.Archives;
using UnitFramework.Utils;

namespace UnitFramework.Runtime
{
    public sealed partial class ArchiveSystem
    {
        private class JsonArchiveSerializer : IArchiveSerializer
        {
            public bool Serialize(Stream stream, object data)
            {
                // 将对象写入对json 对象并保存
                string json = Utility.Json.ToJson(data);
                byte[] bts = System.Text.Encoding.UTF8.GetBytes(json);
                stream.Write(bts,0,bts.Length);
                
                stream.Flush();// 清空缓存
                stream.Close(); // 关闭流
                stream.Dispose();// 清空缓存
                return true;
            }

            public T DeSerialize<T>(Stream stream)
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Close();
                string content = UTF8Encoding.UTF8.GetString(bytes);
                return Utility.Json.ToObject<T>(content);
            }
            
        }
    }
   
}
