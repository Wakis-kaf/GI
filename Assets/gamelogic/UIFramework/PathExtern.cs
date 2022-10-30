using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class PathExtern
{
    /// <summary>
    /// 路径格式化；/ 替换 \\ 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string PathFormat(this string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        path = path.Replace("\\", "/");
        return path;
    }

    /// <summary>
    /// 获取当前路径的父目录
    /// </summary>
    /// <returns></returns>
    public static string GetDirectory(this string str)
    {
        string s = str.PathFormat();
        s = s.TrimEnd('/');
        int index = s.LastIndexOf('/');
        return s.Substring(0, index);
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

    public static string TrimPrefix(this string path, string end)
    {
        if (string.IsNullOrEmpty(path)) return path;
        int index = path.IndexOf(end);
        path = path.Substring(index);
        return path;
    }

    /// <summary>
    /// 修剪后缀格式
    /// </summary>
    /// <param name="path"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static string TrimSuffix(this string path, string end)
    {
        if (string.IsNullOrEmpty(path)) return path;
        int index = path.LastIndexOf(end);
        path = path.Substring(0, index);
        return path;
    }
}
