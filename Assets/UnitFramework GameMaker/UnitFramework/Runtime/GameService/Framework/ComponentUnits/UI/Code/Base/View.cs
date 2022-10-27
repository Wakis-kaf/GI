using System;
using System.Collections;
using System.Collections.Generic;
using UnitFramework.Runtime.UIComponents;
using UnityEngine;

namespace UnitFramework.Runtime
{
    class TipPanelPool
    {
        public bool HasFree => _freeTipPanels.Count > 0;
        public TipPanelManager tipPanelManager { get; private set; }
        private Type _targetType;
        private TipPanel _currentTipPanel;
        private float _durationTimer;

        /// <summary>
        /// 当前所有的tipPanels 
        /// </summary>
        public List<TipPanel> tipPanels = new List<TipPanel>(20);

        /// <summary>
        /// 剩余的空闲TipPanels
        /// </summary>
        private Stack<TipPanel> _freeTipPanels = new Stack<TipPanel>(20);

        /// <summary>
        /// 消息列表
        /// </summary>
        public Queue<string> messages = new Queue<string>();

        /// <summary>
        /// 对象消息队列
        /// </summary>
        public Queue<object> dataMessages = new Queue<object>();

        public TipPanelPool(TipPanelManager mgr, Type type)
        {
            this.tipPanelManager = mgr;
            _targetType = type;
        }

        public TipPanel GetFreePanel()
        {
            TipPanel tip;
            if (!HasFree)
            {
                // 创建Panel
                TipPanel tipPanelTemplate = tipPanelManager.GetTipTemplate(_targetType);
                if (tipPanelTemplate == null)
                {
                    Log.Error($"创建弹框失败!没有找到模板{_targetType.ToString()}");
                    return null;
                }

                // 实例化Tip
                tip =
                    GameObject.Instantiate(tipPanelTemplate, tipPanelManager.view.transform);
                tip.OnClose += () =>
                {
                    // 重置定时器
                    _durationTimer = 0;
                    _currentTipPanel = null;
                    _freeTipPanels.Push(tip);
                };
                tip.gameObject.SetActive(true);
                tipPanelManager.view.AddViewComponent(tip);
                AddPanel(tip);
                return tip;
            }

            tip = _freeTipPanels.Pop();
            tip.gameObject.SetActive(true);
            return tip;
        }

        public TipPanel AddPanel(TipPanel panel)
        {
            if (tipPanels.Contains(panel)) return panel;
            tipPanels.Add(panel);
            _freeTipPanels.Push(panel);
            return panel;
        }

        private bool IsAllowPop()
        {
            bool allowPop = false;
            // 每次取出一条消息进行弹出
            if (tipPanelManager.popType == TipPanelManager.EPopType.Directly)
            {
                // 直接弹出
                allowPop = true;
            }
            else if (tipPanelManager.popType == TipPanelManager.EPopType.OneByOne)
            {
                allowPop = _currentTipPanel == null;
            }

            bool isTimerOver = _durationTimer <= 0;
            if (!isTimerOver)
            {
                _durationTimer -= Time.deltaTime;
                _durationTimer = Mathf.Max(_durationTimer, 0f);
            }

            // 如果没有消息，直接退出
            //if (messages.Count == 0) return false;
            return isTimerOver && allowPop;
        }

        private void PopMessage()
        {
            // 获取弹框
            TipPanel freePanel = GetFreePanel();
            string message = messages.Dequeue();
            freePanel.SendContent(message);
            _currentTipPanel = freePanel;
            // 重置定时器
            _durationTimer = tipPanelManager.duration;
        }

        private void PopData()
        {
            // 获取弹框
            TipPanel freePanel = GetFreePanel();
            object message = dataMessages.Dequeue();
            freePanel.SendContent(message);
            _currentTipPanel = freePanel;
            // 重置定时器
            _durationTimer = tipPanelManager.duration;
        }

        public void Update()
        {
            if (IsAllowPop() && messages.Count > 0)
            {
                // Pop Message
                PopMessage();
            }

            if (IsAllowPop() && dataMessages.Count > 0)
            {
                // Pop Data
                PopData();
            }
        }
    }

    [System.Serializable]
    class TipPanelManager
    {
        public View view { get; private set; }

        public enum EPopType
        {
            // 直接弹出
            Directly,

            // 一个接一个弹出，上一个弹框关闭之后才进行下一个弹框
            OneByOne
        }

        private Dictionary<Type, TipPanelPool> _type2TipPool = new Dictionary<Type, TipPanelPool>();
        private Dictionary<Type, TipPanel> _type2TipPanelTemplate = new Dictionary<Type, TipPanel>();
        [SerializeField] private EPopType _popType;
        public EPopType popType => _popType;

        /// <summary>
        /// 过渡时间
        /// </summary>
        /// 
        public float duration => _duration;

        [SerializeField] private float _duration = .2f;

        public TipPanelManager(View view)
        {
            this.view = view;
        }

        /// <summary>
        /// 弹出消息
        /// </summary>
        /// <param name="content"></param>
        public void PopTip<T>(string content) where T : TipPanel
        {
            Type type = typeof(T);
            PopTip(type, content);
        }

        public void PopTip<T>(object data) where T : TipPanel
        {
            Type type = typeof(T);
            PopTip(type, data);
        }

        public void RegisterTipTemplate<T>(TipPanel template) where T : TipPanel
        {
            Type type = typeof(T);
            if (_type2TipPanelTemplate.ContainsKey(type)) return;
            _type2TipPanelTemplate.Add(type, template);
        }

        public TipPanel GetTipTemplate(Type type)
        {
            return _type2TipPanelTemplate[type];
        }

        public void PopTip(Type type, string content)
        {
            if (!IsCorrectType(type)) return;
            if (!_type2TipPool.ContainsKey(type))
            {
                _type2TipPool.Add(type, new TipPanelPool(this, type));
            }

            _type2TipPool[type].messages.Enqueue(content);
        }

        public void PopTip(Type type, object data)
        {
            if (!IsCorrectType(type)) return;
            if (!_type2TipPool.ContainsKey(type))
            {
                _type2TipPool.Add(type, new TipPanelPool(this, type));
            }

            _type2TipPool[type].dataMessages.Enqueue(data);
        }

        private bool IsCorrectType(Type type)
        {
            return type.IsSubclassOf(typeof(TipPanel));
        }


        public void Update()
        {
            // 取出消息队列
            foreach (var pool in _type2TipPool)
            {
                pool.Value.Update();
            }
        }
    }

    public class View : ViewComponent
    {
        [SerializeField] [Header("提示框管理")] TipPanelManager _tipPanelManager;

        public View()
        {
            _tipPanelManager = new TipPanelManager(this);
        }

        protected override void Start()
        {
            base.Start();
            StartCoroutine(TipHandling_Coroutine());
        }

        public void RegisterTipTemplate<T>(TipPanel template) where T : TipPanel
        {
            _tipPanelManager.RegisterTipTemplate<T>(template);
        }

        public void PopTip<T>(string content) where T : TipPanel
        {
            _tipPanelManager.PopTip<T>(content);
        }

        public void PopTip<T>(object data) where T : TipPanel
        {
            _tipPanelManager.PopTip<T>(data);
        }

        public void PopTip(Type type, string content)
        {
            _tipPanelManager.PopTip(type, content);
        }

        IEnumerator TipHandling_Coroutine()
        {
            while (true)
            {
                _tipPanelManager.Update();
                yield return null;
            }
        }
    }
}