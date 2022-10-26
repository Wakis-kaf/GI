using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitFramework.Runtime.Archives;

namespace UnitFramework.Runtime
{
    public interface IArchiveSerializer 
    {
        /// <summary>
        /// 序列化数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        bool Serialize(Stream stream, object data);
        /// <summary>
        /// 反序列化数据
        /// </summary>
        /// <param name="stream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T DeSerialize<T>(Stream stream);


    }
}
