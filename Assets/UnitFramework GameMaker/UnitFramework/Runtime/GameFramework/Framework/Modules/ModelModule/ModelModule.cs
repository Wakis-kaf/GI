using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 模型模块
    /// </summary>
    [AutoRegisterModule]
    public class ModelModule : Module
    {
        private List<BlackBoard> m_BlackBoards = new List<BlackBoard>();
        private Dictionary<int,BlackBoard> mId2BlacBoardMap = new Dictionary<int, BlackBoard>();
        
        public BlackBoard GlobalBlackBoard => GetBlackBoard("GlobalBlackBoard");
        public override int Priority => (int)GameFrameworkConfig.FrameModuleConfig.ModulePriority.ModelModule;
        
        /// <summary>
        /// 获取黑板
        /// 如果黑板不存在就创建黑板
        /// </summary>
        /// <param name="blackBoardName"></param>
        /// <returns></returns>
        
        public BlackBoard GetBlackBoard(string blackBoardName)
        {
            for (int i = 0; i < m_BlackBoards.Count; i++)
            {
                if (m_BlackBoards[i].name.Equals(blackBoardName)) return m_BlackBoards[i];
            }
            // 创建一个新的黑板
            return CreateAndAddBlackBoard(blackBoardName);
        }
        
        public BlackBoard CreateAndAddBlackBoard(string blackBoardName)
        {
            return  AddBlackBoard(new BlackBoard(blackBoardName));
        }
        
        public BlackBoard AddBlackBoard(BlackBoard blackBoard)
        {
            if (mId2BlacBoardMap.ContainsKey(blackBoard.Id)) return mId2BlacBoardMap[blackBoard.Id];
            mId2BlacBoardMap.Add(blackBoard.Id,blackBoard);
            m_BlackBoards.Add(blackBoard);
            return blackBoard;
        }
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            for (int i = m_BlackBoards.Count - 1; i >= 0; i--)
            {
                DestroyBlackBoardAt(i);
            }
        }

        public void DestroyBlackBoardAt(int index)
        {
            
            int length = m_BlackBoards.Count;
            if(index == -1 || length == 0) return;
            BlackBoard bb = m_BlackBoards[index];
            m_BlackBoards[index] = m_BlackBoards[length - 1];
            m_BlackBoards[length - 1] = bb;
            bb.Dispose();
            m_BlackBoards.RemoveAt(length-1);
            mId2BlacBoardMap.Remove(bb.Id);
        }
        
        public BlackBoard FindBlackBoard(int id)
        {
            mId2BlacBoardMap.TryGetValue(id, out var res);
            return res;
        }
        
        public void DestroyBlackBoard(BlackBoard blackBoard)
        {
            int index = m_BlackBoards.IndexOf(blackBoard);
            DestroyBlackBoardAt(index);
         
        }
       

    }

}
