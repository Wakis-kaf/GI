using UnityEngine;

namespace GFramework.UI
{
    public abstract class BaseView : IView
    {
        // ui游戏物体
        public GameObject gameObject { get; private set; }

        // ui变换组件
        public Transform transform { get; private set; }

        // mono组件
        protected UIBinder uiBinder;

        public bool IsHided { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Show()
        {
            this.IsDisposed = false;
            this.IsHided = false;
            this.OnPreShow();
            this.gameObject.SetActive(true);
            this.OnShow();
        }

        public void Hide()
        {
            this.OnPreHide();
            this.gameObject.SetActive(false);
            this.OnHided();
            this.IsHided = true;
        }

        public void Close()
        {
            this.OnClosing();
            this.gameObject.SetActive(false);
            this.OnClosed();
        }

        public void Dispose()
        {
            this.gameObject.SetActive(false);
            this.IsDisposed = true;
            UnityEngine.GameObject.Destroy(this.gameObject);
        }

        // 获取预制体UGUI组件
        protected T GetVar<T>(int index) where T : Component
        {
            return this.uiBinder.GetVar(index).component as T;
        }

        // 获取预制体UIContainer组件，因为需要递归创建对应的view对象
        protected T1 GetBinder<T1>(int index) where T1 : BaseView, new()
        {
            UIVar var = this.uiBinder.GetVar(index);
            T1 view = UIMgr.NewUI<T1>();
            view.BindGO(var.gameObject, true);
            return view;
        }

        public void BindGO(GameObject go, bool exist = false)
        {
            this.gameObject = go;
            this.transform = go.transform;
            this.uiBinder = go.GetComponent<UIBinder>();
            if (!exist)
                this.transform.SetParentOfUI(this.uiBinder.layer, this.uiBinder.node);
            Load();
        }

        private void Load()
        {
            this.BindVars();
            this.BindEvents();
            this.OnLoaded();
        }

        // 获取预制体路径
        public virtual string BindPath() { return null; }
        // 获取uicontainer组件
        protected virtual void BindVars() { }
        /// <summary>
        /// ui类生成事件，以下按调用顺序
        /// </summary>
        protected virtual void BindEvents() { }
        protected virtual void OnLoaded() { }
        protected virtual void OnPreShow() { }
        protected virtual void OnShow() { }
        protected virtual void OnPreHide() { }
        protected virtual void OnHided() { }
        protected virtual void OnClosing() { }
        protected virtual void OnClosed() { }
    }
}