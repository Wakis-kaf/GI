using System.Collections.Generic;
using UnitFramework.Utils;


namespace UnitFramework.Runtime
{
    /// <summary>
    /// 黑板，用于记录数据
    /// </summary>
    public class BlackBoard : FrameObject
    {
        private int m_Id;
        private string m_Name;
        private Dictionary<string, Model> mName2ModelMap = new Dictionary<string, Model>();
        public string name
        {
            get => m_Name;
        }
        public int Id
        {
            get => m_Id;
        }
        public BlackBoard()
        {
            InitBlackBoard();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        public BlackBoard(string name)
        {
            m_Name = name;
            InitBlackBoard();
        }

        private void InitBlackBoard()
        {
            m_Id = Utility.IDGenerator.GetIntGuidID();
        }

        public Model GetAndCreateModel(string modelName)
        {
            mName2ModelMap.TryGetValue(modelName, out var res);
            if (res == null)
            {
                res = new  Model(this);
                mName2ModelMap.Add(modelName,res);
            }
            return res;
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            mName2ModelMap.Clear();
            mName2ModelMap = null;
        }
    }
}