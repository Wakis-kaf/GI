using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UGFramework.Util
{
    public class FileUtil
    {
        /// <summary>
        /// 递归创建文件
        /// </summary>
        /// <param name="fullPath"></param>
        public static void DepCreateFile(string fullPath)
        {
            var p = fullPath.PathFormat();
            var steps = p.Split('/');
            int stepNum = steps.Length;
            string path = string.Empty;
            for (int i = 0; i < stepNum; ++i)
            {
                if (i == stepNum - 1)
                {
                    File.Create(fullPath).Dispose();
                    break;
                }
                path += steps[i];
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path += '/';
            }
        }
    }
}