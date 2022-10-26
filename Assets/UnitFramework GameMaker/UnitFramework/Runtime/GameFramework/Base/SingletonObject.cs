namespace UnitFramework.Runtime
{
    /// <summary>
    /// 单例对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonObject<T> : FrameObject where T : class, new()
    {
        private static T m_Instance;
        public static T Instance => m_Instance;
        
        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            if (ReferenceEquals(m_Instance, this)) m_Instance = null;
        }
        
        public static T CreateInstance()
        {
            if (!ReferenceEquals(m_Instance, null)) return m_Instance;
            m_Instance = new T();
            return m_Instance;
        }

       
    }
}