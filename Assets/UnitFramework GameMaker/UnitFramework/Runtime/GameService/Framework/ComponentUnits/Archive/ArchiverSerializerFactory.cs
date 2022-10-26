using System;
using UnitFramework.Runtime.Archives;

namespace UnitFramework.Runtime
{
    public sealed partial class ArchiveSystem
    {
        private static class ArchiverSerializerFactory
        {
            public static IArchiveSerializer CreatSerializer(ArchiveSystem.SaveMode saveMode)
            {
                switch (saveMode)
                {
                    case ArchiveSystem.SaveMode.Binary:
                        return  new BinaryArchiveSerializer();
                        break;
                    case ArchiveSystem.SaveMode.Json:
                        return new JsonArchiveSerializer();
                    case ArchiveSystem.SaveMode.Xml :
                        break;
                        
                }

                return default;
            }
        }

     
    }

  
}