using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitFramework.Runtime.Archives;

namespace UnitFramework.Runtime
{
    public sealed partial class ArchiveSystem
    {
        private class ArchiveManager : Unit
        {
            private IArchiveSerializer m_Helper;
            public IArchiveSerializer SetHelper(IArchiveSerializer helper)
            {
                m_Helper = helper;
                return m_Helper;
            }
            /// <summary>
            /// 序列化数据到目标流中
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="data"></param>
            public bool Serialize(Stream stream, object data)
            {
                if (m_Helper != null)
                {
                    return m_Helper.Serialize(stream, data);
                }
                return false;
            }
            /*/// <summary>
            /// 序列化自定义数据到存档中
            /// </summary>
            /// <param name="archive"></param>
            /// <typeparam name="T"></typeparam>
            public void Serialize<T>(Archive<T> archive)
            {
                if(m_Helper!= null)
                    archive.SerializeCustomData(m_Helper);
                
            }*/

            /// <summary>
            /// 反序列化数据
            /// </summary>
            /// <param name="stream"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T DeSerialize<T>(Stream stream)
            {
                if (m_Helper != null)
                {
                    return m_Helper.DeSerialize<T>(stream);
                }
                return default;
            }

            
          
        }
    }
       
}
