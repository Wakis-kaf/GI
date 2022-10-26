using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public sealed partial class ArchiveSystem
    {
        private static class ArchiveFilePrefix
        {
            public  static string GetPrefix(SaveMode saveMode)
            {
                switch (saveMode)
                {
                    case SaveMode.Binary:
                        return ".archive";
                    case SaveMode.Json:
                        return ".json";
                    case SaveMode.Xml:
                        return ".xml";

                }
                return ".archive";
            }
        }
    }
  
}
