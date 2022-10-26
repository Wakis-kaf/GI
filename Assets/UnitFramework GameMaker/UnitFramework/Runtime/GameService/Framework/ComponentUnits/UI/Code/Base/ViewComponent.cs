using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace UnitFramework.Runtime
{
    public enum EViewComponentInit
    {
        Default,
        ShowAll,
        HideAll,
        Custom
    }
    
    [AddComponentMenu("EKaf Framework/BaseViewComponent")]
    public class ViewComponent : MonoUnit,IViewer
    {
        [System.Serializable]
        private class ViewsHistory
        {
            public ViewComponent[] visibleViewComponents;
            public ViewComponent[] inVisibleViewComponents;

            public ViewsHistory()
            {
            }

            public ViewsHistory(ViewComponent[] visibleVCmp, ViewComponent[] inVisibleVCmp)
            {
                this.visibleViewComponents = visibleVCmp;
                this.inVisibleViewComponents = inVisibleVCmp;
            }
        }

        //public static LogHelper LogHelper = new LogHelper();
        public Type type { get; private set; }
        public string typeStr { get; private set; }
        public bool IsVisible { get; private set; }

        public ViewComponent Parent => _parent;
        private ViewComponent _parent;

        public virtual Transform rootTransform => transform;

        public bool HasParent => Parent != null;

        [Header("视图组件设置")] [SerializeField] private string _componetName;

        public virtual string componentName
        {
            get => _componetName;
            set { _componetName = value; }
        }

        public string componentDescription;

        public virtual bool IsIgnoreViewSwitchHide
        {
            get => _isIgnoreViewSwitchHide;
            set => _isIgnoreViewSwitchHide = value;
        }

        private bool _isIgnoreViewSwitchHide = false;

        public virtual bool IsIgnoreHistory
        {
            get => _isIgnoreHistory;
            set => _isIgnoreHistory = value;
        }

        private bool _isIgnoreHistory;
        [Header("Debug")] [SerializeField] private List<ViewComponent> _childComponents = new List<ViewComponent>(100);

        [SerializeField] private EViewComponentInit _viewComponentInitType;

        [SerializeField] private List<ViewComponent> _startComponents = new List<ViewComponent>();

        // 显示的视图
        //[SerializeField]
        private List<ViewComponent> _visibleComponents = new List<ViewComponent>();

        //[SerializeField]
        // 隐藏的视图
        private List<ViewComponent> _inVisibleComponents = new List<ViewComponent>();

        /// <summary>
        /// 历史记录
        /// </summary>
        /*        [SerializeField]
                [Header("Debug；EditDisbale")]*/
        private Stack<ViewsHistory> _histories = new Stack<ViewsHistory>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ViewComponent()
        {
            type = GetType();
            typeStr = type.ToString();
        }

        protected virtual void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.TryGetComponent<ViewComponent>(out ViewComponent viewComponent))
                {
                    // 添加视图组件
                    AddViewComponent(viewComponent);
                    Debug.Log(viewComponent.componentName);
                }
            }

            IsVisible = enabled;

        }

        protected virtual void Start()
        {
            // 显示所有的开始组件
            InitStartComponent();
            // 初始化历史记录栈
            InitHistory();
        }

        protected virtual void Update()
        {
        }

        protected virtual void OnValidate()
        {
            if (componentName != _componetName)
            {
                _componetName = componentName;
            }
        }

        protected virtual void OnDestory()
        {
            if (HasParent)
                Parent.RemoveViewComponent(this);
        }

        private void OnEnable()
        {
            IsVisible = true;
        }

        private void OnDisable()
        {
            IsVisible = false;
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual void OnAdd(ViewComponent parent)
        {
        }

        public virtual void OnRemove(ViewComponent parent)
        {
        }

        /// <summary>
        /// 初始化开始组件
        /// </summary>
        private void InitStartComponent()
        {
            if (_viewComponentInitType == EViewComponentInit.Default)
            {
                // 更新视图组件的可见性
                UpdateViewComponentsVisible();
                return;
            }
            else if (_viewComponentInitType == EViewComponentInit.ShowAll)
            {
                _startComponents = _childComponents;
            }
            else if (_viewComponentInitType == EViewComponentInit.HideAll)
            {
                _startComponents.Clear();
            }

            foreach (var cmp in _childComponents)
            {
                if (_startComponents.Contains(cmp))
                {
                    cmp.Show();
                }
                else
                {
                    cmp.Hide();
                }
            }

            // 更新视图组件的可见性
            UpdateViewComponentsVisible();
        }

        /// <summary>
        /// 初始化历史记录
        /// </summary>
        private void InitHistory()
        {
            _histories.Clear();
        }

        /// <summary>
        ///  显示视图
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isHideCurrent">是否隐藏当前的视图</param>
        /// <param name="isHistoryStack">是否计入历史</param>
        public void ShowViews<T>(bool isHideCurrent = true, bool isHistoryStack = true) where T : ViewComponent
        {
            List<T> components = GetViewComponentsInChildren<T>();
            if (isHistoryStack)
            {
                HistorySave();
            }

            if (isHideCurrent)
            {
                foreach (var item in _visibleComponents)
                {
                    item.Hide();
                }
            }

            foreach (var component in components)
            {
                component.Show();
            }

            UpdateViewComponentsVisible();
        }

        /// <summary>
        /// 显示视图
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isHideCurrent"></param>
        /// <param name="isHistoryStack"></param>
        public void ShowView<T>(bool isHideCurrent = true, bool isHistoryStack = true) where T : ViewComponent
        {
            T component = GetViewComponentInChildren<T>();
            if (isHistoryStack)
            {
                HistorySave();
            }

            if (isHideCurrent)
            {
                foreach (var item in _visibleComponents)
                {
                    if (!item.IsIgnoreViewSwitchHide)
                        item.Hide();
                }
            }

            component.Show();
            UpdateViewComponentsVisible();
        }

        /// <summary>
        /// 保存历史
        /// </summary>
        private void HistorySave()
        {
            UpdateViewComponentsVisible();
            _histories.Push(new ViewsHistory(_visibleComponents.ToArray(), _inVisibleComponents.ToArray()));
        }

        /// <summary>
        /// 历史回退
        /// </summary>
        public void HistoryBack()
        {
            if (_histories.Count == 0)
            {
                Log.Warning("回退历史失败，无历史记录!");
                return;
            }

            ViewsHistory history = _histories.Pop();
            _visibleComponents.Clear();
            _inVisibleComponents.Clear();
            foreach (var component in history.visibleViewComponents)
            {
                component.Show();
                _inVisibleComponents.Add(component);

                Log.DebugInfo($"回退可见视图: {component.name}");
            }

            foreach (var component in history.inVisibleViewComponents)
            {
                component.Hide();
                _inVisibleComponents.Add(component);

                Log.DebugInfo($"回退不可见视图: {component.name}");
            }

            Log.DebugInfo("历史回退成功!");
        }

        /// <summary>
        /// 更新子组件的可见性
        /// </summary>
        private void UpdateViewComponentsVisible()
        {
            _inVisibleComponents.Clear();
            _visibleComponents.Clear();
            //Debug.Log(name + _childComponents.Count);
            foreach (var component in _childComponents)
            {
                if (component.IsIgnoreHistory) continue;
                if (component.IsVisible)
                {
                    _visibleComponents.Add(component);
                }
                else
                {
                    _inVisibleComponents.Add(component);
                }
            }
        }


        /// <summary>
        /// 显示视图
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ShowViews<T>(string name) where T : ViewComponent
        {
            List<T> components = GetViewComponentsInChildren<T>();
            foreach (var component in components)
            {
                if (component.componentName.Equals(name))
                    component.Show();
            }
        }


        /// <summary>
        /// 隐藏所有视图
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void HidenViews<T>() where T : ViewComponent
        {
            List<T> components = GetViewComponentsInChildren<T>();
            foreach (var component in components)
            {
                component.Hide();
            }
        }

        /// <summary>
        /// 隐藏所有视图
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        public void HidenViews<T>(string name) where T : ViewComponent
        {
            List<T> components = GetViewComponentsInChildren<T>();
            foreach (var component in components)
            {
                if (component.componentName.Equals(name))
                    component.Hide();
            }
        }

        /// <summary>
        /// 获取指定类型的所有孩子组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetViewComponentsInChildren<T>() where T : ViewComponent
        {
            List<T> res = new List<T>(100);
            foreach (var child in _childComponents)
            {
                if (child is T)
                {
                    res.Add(child as T);
                }
            }

            return res;
        }

        /// <summary>
        /// 获取指定类型的所有孩子组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetViewComponentsInChildren<T>(string name) where T : ViewComponent
        {
            List<T> res = new List<T>(100);
            foreach (var child in _childComponents)
            {
                if (child is T && child.componentName == name)
                {
                    res.Add(child as T);
                }
            }

            return res;
        }

        /// <summary>
        /// 获取指定类型的孩子组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetViewComponentInChildren<T>() where T : ViewComponent
        {
            foreach (var child in _childComponents)
            {
                if (child is T)
                {
                    return child as T;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取指定类型的孩子组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetViewComponentInChildren<T>(string name) where T : ViewComponent
        {
            foreach (var child in _childComponents)
            {
                if (child is T && child.componentName == name)
                {
                    return child as T;
                }
            }

            return null;
        }

        /// <summary>
        /// 添加视图组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewComponent"></param>
        /// <returns></returns>
        public T AddViewComponent<T>(T viewComponent) where T : ViewComponent
        {
            if (ReferenceEquals(viewComponent, this)) return null;
            if (_childComponents.Contains(viewComponent))
            {
                Log.DebugInfo($"添加组件失败，组件重复添加{viewComponent.typeStr}");


                return viewComponent;
            }

            _childComponents.Add(viewComponent);
            viewComponent.SwitchParent(this);
            viewComponent.OnAdd(this);
            return viewComponent;
        }

        /// <summary>
        /// 移除视图组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewComponent"></param>
        public void RemoveViewComponent<T>(T viewComponent) where T : ViewComponent
        {
            if (_childComponents.Contains(viewComponent))
            {
                _childComponents.Remove(viewComponent);
                viewComponent.OnRemove(this);
                viewComponent._parent = null;

                Log.DebugInfo($"移除组件成功{viewComponent.typeStr}");
            }
        }

        /// <summary>
        /// 切换父节点
        /// </summary>
        /// <param name="viewComponent"></param>
        private void SwitchParent(ViewComponent viewComponent)
        {
            if (HasParent)
            {
                Parent.RemoveViewComponent(this);
            }

            _parent = viewComponent;
            transform.SetParent(_parent?.rootTransform);
        }

        public virtual void OnCtrViewModelConnected(CVBlackBoard cvBlackBoard)
        {
            
        }

        public virtual void OnCtrViewModelDisConnected(CVBlackBoard cvBlackBoard)
        {
          
        }

        public virtual void OnCtrModelConnected(IController controller)
        {
        
        }

        public virtual void OnCtrModelDisConnected(IController controller)
        {
            
        }
    }
}