using System;
using UnityEngine;
using LitJson;
using LitJson.Extensions;
using UnitFramework.Utils;

namespace UnitFramework.Runtime.Archives
{
    /// <summary>
    /// 存档文件 ,  每一份存档都会单独序列化为一个文件保存到目标路径下
    /// </summary>
    [System.Serializable]
    public class Archive<T>
    {
        [SerializeField, JsonSerializer]
        private int m_Id;
        [SerializeField, JsonSerializer]
        private string m_Name;
        [SerializeField, JsonSerializer]
        private string m_Version;
        private string m_CustomDataJson;
        [SerializeField, JsonSerializer]
        private T m_Data;
        [SerializeField, JsonSerializer]
        private DateTime m_CreateTime; //  存档创建时间
        [SerializeField, JsonSerializer]
        private DateTime m_lastUpdateTime; // 存档上次更新时间

        [JsonIgnore] public int Id => m_Id;
        [JsonIgnore] public string Version => m_Version; // 存档版本 
        [JsonIgnore] public string Name => m_Name; // 存档名字
        //public string CustomDataJson=>m_CustomDataJson;

        [JsonIgnore] public DateTime CreateTime => m_CreateTime; //  存档创建时间
        [JsonIgnore] public DateTime LastUpdateTime => m_lastUpdateTime; // 存档上次更新时间

        public Archive()
        {
        }

        public Archive(T data, string version, string name)
        {
            SetData(data);
            this.m_Version = version;
            this.m_Name = name;
            m_Id = Utility.IDGenerator.GetIntGuidID(); // UUID  生成独一无二的UU ID
            m_lastUpdateTime = m_CreateTime = DateTime.Now;
        }

        public Archive(string version, string name)
        {
            this.m_Version = version;
            this.m_Name = name;
            m_Id = Utility.IDGenerator.GetIntGuidID(); // UUID  生成独一无二的UU ID
            m_lastUpdateTime = m_CreateTime = DateTime.Now;
        }

        public void Save()
        {
            ArchiveSystem.Instance.SaveArchive(this, true); // 保存文档
        }

        public T SetData(T data)
        {
            m_Data = data;
            return m_Data;
        }

        public T GetData()
        {
            return m_Data;
        }

        public string GetArchiveFileName()
        {
            return m_Id.ToString();
        }

        public void SetName(string name)
        {
            m_Name = name;
        }

        /// <summary>
        /// 序列化自定义数据
        /// </summary>
        /// <param name="mHelper"></param>
        // public void SerializeCustomData(IArchiveSerializer mHelper)
        // {
        //     if(m_Data == null) return;
        //     m_CustomDataJson = mHelper.SerializeToJson(m_Data);
        // }

        /// <summary>
        /// 存档被序列化之后调用
        /// </summary>
        public void OnSerialize()
        {
            // 更新保存时间
            m_lastUpdateTime = System.DateTime.Now;
        }

        /// <summary>
        /// 存档被反序列化之后调用
        /// </summary>
        public void OnDeSerialize()
        {
        }
    }
}