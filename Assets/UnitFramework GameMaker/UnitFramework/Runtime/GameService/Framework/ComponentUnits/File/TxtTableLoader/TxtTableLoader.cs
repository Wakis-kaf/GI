using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class TxtTableLoader : SingletonComponentUnit<TxtTableLoader>
    {
        public override string ComponentUnitName { get=>"TxtTableLoader"; }
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string [][] ReadTxtTableFile(string path, string name,bool includeTitle = false)
        {
            string fileFullName = string.Empty;
            List<string[]> set = new List<string[]>();
            
            if (!path.EndsWith("/"))
                fileFullName = string.Concat(path, "/", name);
            else
            {
                fileFullName = string.Concat(path, name);
            }

         
            try
            {
                StreamReader streamReader = new StreamReader(fileFullName);
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {   // 4 将每行添加到文件List中
                    line = line.Trim();
                    if(string.IsNullOrWhiteSpace(line)) continue;
                    // 如果是注释 就跳过
                    if(!includeTitle && line.StartsWith("#")) continue;
                    set.Add(line.Split(','));
                }
                streamReader.Close();
                return set.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("read file error! message: {0}", e.Message);
                //throw;
                return set.ToArray();
            }
        }
    }
}