using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public interface IViewer : IUnitBehaviour
    {
        /// <summary>
        /// 当控制器和当前视图通过数据绑定时调用
        /// </summary>
        /// <param name="cvBlackBoard">控制器和视图的中间数据黑板</param>
        public void OnCtrViewModelConnected(CVBlackBoard cvBlackBoard);
        /// <summary>
        /// 当控制器和当前视图通过数据绑定断开时调用
        /// </summary>
        /// <param name="cvBlackBoard">控制器和视图的中间数据黑板</param>
        public void OnCtrViewModelDisConnected(CVBlackBoard cvBlackBoard);
        /// <summary>
        /// 当控制器和当前视图绑定时调用
        /// </summary>
        /// <param name="IController">绑定的控制器</param>
        public void OnCtrModelConnected(IController controller);
        /// <summary>
        /// 当控制器和当前视图绑定断开时调用
        /// </summary>
        /// <param name="IController">解绑的控制器</param>
        public void OnCtrModelDisConnected(IController controller);
    }
}
