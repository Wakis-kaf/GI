using System.IO;
using System.Xml.Serialization;

namespace UnitFramework.Utils
{
    public static partial class Utility
    {
        public  static class  Convert
        {
            public static T DeepCopyBYXML<T>(T obj)
            {
                object retval;
                using (MemoryStream ms = new MemoryStream())
                {
                    XmlSerializer xml = new XmlSerializer(typeof(T));
                    xml.Serialize(ms, obj);
                    ms.Seek(0, SeekOrigin.Begin);
                    retval = xml.Deserialize(ms);
                    ms.Close();
                }
                return (T)retval;
            }

        }
    }
}