using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GFramework.UI
{
    // UI面板接口
    public interface IView
    {
        string BindPath();
        void Show();
        void Hide();
        void Close();
        void Dispose();
    }
}