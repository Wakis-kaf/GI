using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GFramework.Util
{
    public static class PathExtern
    {
        public static string PathFormat(this string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            path = path.Replace("\\", "/");
            return path;
        }

        // 获取路径前缀
        public static string GetFirstFieldName(this string path, char mark = '/')
        {
            if (string.IsNullOrEmpty(path)) return path;
            int index = path.IndexOf(mark);
            path = path.Substring(0, index < 0 ? 0 : index);
            return path;
        }

        // 获取路径后缀文件名
        public static string GetLastFieldName(this string path, char mark = '/')
        {
            if (string.IsNullOrEmpty(path)) return path;
            int index = path.LastIndexOf(mark);
            path = path.Substring(index + 1);
            return path;
        }

        // 获取路径后缀文件名，不含文件格式
        public static string GetLastFieldNameWithoutSuffix(this string path, char mark = '/')
        {
            if (string.IsNullOrEmpty(path)) return path;
            int index = path.LastIndexOf(mark);
            path = path.Substring(index + 1);
            return path;
        }

        public static string TrimEnd(this string path, string end)
        {
            if (string.IsNullOrEmpty(path)) return path;
            int index = path.LastIndexOf(end);
            path = path.Substring(0, index);
            return path;
        }
    }
}