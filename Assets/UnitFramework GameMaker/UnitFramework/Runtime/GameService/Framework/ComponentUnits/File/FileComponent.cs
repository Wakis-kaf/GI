using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 文件系统： 功能如下
    /// CSV 读取，存储
    /// JSON 读取，存储
    /// Txt 读取， 存储
    /// EXCEL 读取存储
    /// </summary>
    public class FileComponent : SingletonComponentUnit<FileComponent>
    {
        public override string ComponentUnitName { get=>"FileComponent"; }
        public static JsonLoader JsonLoader => global::JsonLoader.Instance;
        public static TxtTableLoader TxtTableLoader => TxtTableLoader.Instance;
        public static ExcelLoader ExcelLoader => ExcelLoader.Instance;

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="isOverride"></param>
        public static void SaveFile(string path, string name, string content, bool isOverride = true)
        {
            StreamWriter sw;
            // 判断路径是否存在
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            FileInfo fileInfo = new FileInfo(path+"//"+name);
            if (!fileInfo.Exists)
            {
                // 创建文件
                sw = fileInfo.CreateText();
            }
            else
            {
                if (isOverride)
                {
                    // 删除文件
                    fileInfo.Delete();
                    // 创建文件
                    sw = fileInfo.CreateText();
                }
                else
                {
                    sw = fileInfo.AppendText();
                }
            }
            //以行的形式写入信息  
            sw.WriteLine(content);  
            //关闭流  
            sw.Close();  
            //销毁流  
            sw.Dispose();
            return ;
        }

        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ReadFile(string path, string name)
        {
            string fileFullName = string.Empty;
            if (!path.EndsWith("/"))
                fileFullName = string.Concat(path, "/", name);
            else
            {
                fileFullName = string.Concat(path, name);
            }
            try
            {
                StreamReader streamReader = new StreamReader(fileFullName);
                string jsonStr = streamReader.ReadToEnd();
                streamReader.Close();
                return jsonStr;
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("read file error! message: {0}", e.Message);
                //throw;
                return String.Empty; 
            }
        }
    }
  
}
