namespace UnitFramework.Runtime
{
    [System.Serializable]
    public abstract class Pool
    {
        private string m_PoolName = "ObjectPool";
        // 最多分类24个种类
        public int tagLimitCount = 24;
        // 最多池子数量
        public int poolLimitCount = 1000;
        // 池子初始化大小
        public int poolInitSize = 50;
        public string poolName { get=>m_PoolName; set=>m_PoolName=value; }
        
    }
}