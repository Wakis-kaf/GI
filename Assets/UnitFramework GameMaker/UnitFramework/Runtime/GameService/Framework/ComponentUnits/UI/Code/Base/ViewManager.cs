using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnitFramework.Runtime
{

    /// <summary>
    /// ViewComponent： 所有UI组件的基类,视图组件,由视图组件组成视图（View），由视图拼装成画布视图(CanvasView)
    /// 
    /// View：视图类
    /// CanvasView : 画布视图类
    /// ViewManager:  单例视图管理器
    ///         ViewManager
    ///     |               |
    ///   canvas           canvas
    /// |       |           |
    /// view    view        view
    /// |
    /// viewComponent
    /// </summary>
    ///
    public sealed class ViewManager : ViewComponent
    {
        /// <summary>
        /// 单例模式
        /// </summary>
        public static ViewManager Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// 全部画布
        /// </summary>
        public List<CanvasView> CanvasViews => GetViewComponentsInChildren<CanvasView>();
        Transform _uiRoot;
        public override Transform rootTransform => _uiRoot;
        public Transform dontDestroyTransform;
        protected override void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            base.Awake();
            DontDestroyCanvasView("Console Canvas");
            // 创建根结点UI
        }

        public void DontDestroyCanvasView(CanvasView canvasView)
        {
            if(canvasView == null) return;
            canvasView.isDontDestroy = true;
            canvasView.transform.SetParent(dontDestroyTransform);
        }
        public void DontDestroyCanvasView(string canvasViewName)
        {
            var item = GetViewComponentInChildren<CanvasView>(canvasViewName);
            DontDestroyCanvasView(item);
            
        }

        public void CancelDontDestroyCanvasView(CanvasView canvasView)
        {
            if(canvasView == null) return;
            canvasView.isDontDestroy = false;
            canvasView.transform.parent.SetParent(rootTransform);
        } 
        public void CancelDontDestroyCanvasView(string canvasViewName)
        {
            DontDestroyCanvasView(GetViewComponentInChildren<CanvasView>(canvasViewName));
        }
        public void Refresh()
        {
            Debug.Log($"[UI Refresh] {SceneManager.GetActiveScene().name}");
            _uiRoot = new GameObject($"[{SceneManager.GetActiveScene().name}].UIRoot").transform;
            // 查找所有的画布把画布添加到ViewComponents中
            CanvasView[] canvas = FindObjectsOfType<CanvasView>(true);
            foreach (var item in canvas)
            {
                AddViewComponent(item);
                if (item.isDontDestroy)
                {
                    DontDestroyCanvasView(item);
                }
            }
        }
        private void OnDestroy()
        {
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }

        }
        protected override void OnValidate()
        {
            componentName = "ViewManager";

        }
    }

}
