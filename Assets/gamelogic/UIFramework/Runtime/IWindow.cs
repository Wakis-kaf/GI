using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UGFramework.UI
{
    // UI面板接口
    public interface IWindow
    {
        string BindPath();
        void Show();
        void Hide();
        void Close();
        void Dispose();
    }
}