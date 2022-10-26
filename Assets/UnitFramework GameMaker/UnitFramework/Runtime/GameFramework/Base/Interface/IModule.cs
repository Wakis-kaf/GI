namespace UnitFramework.Runtime
{
    /// <summary>
    /// 模块接口
    /// </summary>
    public interface IModule : IFrameObject
    {
        /// <summary>
        /// 优先级
        /// 优先级高的组件会优先执行生命函数
        /// 同时优先级高的模块会在框架被销毁的时候最后销毁
        /// 默认为 0
        /// </summary>
        public  int Priority { get; }
        /// <summary>
        /// 生命函数，当所有框架都加载完成的时候调用Start 方法
        /// 调用事件为 Unity 启动后的 Awake 中调用
        /// </summary>
        public  void OnFrameStart();
        /// <summary>
        /// 生命函数，组件轮询方法
        /// 调用事件默认为Unity Update ，具体调用事件取决于 Starter 中的UpdateMode
        /// </summary>
        public  void OnFrameUpdate();
        
        /// <summary>
        /// 生命函数，组件固定轮询方法
        /// 调用事件默认为Unity FixedUpdate 
        /// </summary>
        public  void OnFrameFixedUpdate();
        
        /// <summary>
        /// 生命函数，框架注销的时候调用
        /// 默认调用时间为 Starter 对象被销毁的时候调用
        /// </summary>
        public  void OnFrameworkShutdown();
    }
}