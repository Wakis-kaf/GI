using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitFramework.Runtime.UIComponents
{
    /// <summary>
    /// 弹出框
    /// </summary>
    public abstract class TipPanel : ViewComponent
    {
        public enum ECloseType
        {
            /// <summary>
            /// 时间结束自动关闭
            /// </summary>
            TimeOut,

            /// <summary>
            /// 点击关闭
            /// </summary>
            ClickClose,

            /// <summary>
            /// 二者都可以关闭
            /// </summary>
            Both
        }

        public override bool IsIgnoreViewSwitchHide
        {
            get => true;
            set { }
        }

        public override bool IsIgnoreHistory
        {
            get => true;
            set { }
        }

        [Space(10)] [Header("弹出框设置")] public ECloseType closeType;
        public event Action OnClose;

        public virtual void CloseTip()
        {
            Hide();
            OnClose?.Invoke();
        }

        public virtual void SendContent(string content)
        {
        }

        public virtual void SendContent(object data)
        {
        }
    }
}