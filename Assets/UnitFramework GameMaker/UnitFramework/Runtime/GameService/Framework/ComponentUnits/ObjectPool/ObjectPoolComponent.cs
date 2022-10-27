using System.Collections.Generic;

namespace UnitFramework.Runtime
{
    public class ObjectPoolComponent : SingletonComponentUnit<ObjectPoolComponent>
    {
        private Dictionary<string, Pool> m_Name2PoolMap = new Dictionary<string, Pool>();

        public override string ComponentUnitName
        {
            get => "Object Pool";
        }

        public GameObjectPool GameObjectPool => m_DefaultGameObjectPool;
        private GameObjectPool m_DefaultGameObjectPool;

        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            InitGameObjectPool();
        }

        private void InitGameObjectPool()
        {
            m_DefaultGameObjectPool = new GameObjectPool(transform);
            AddPool("GameObjectPool", m_DefaultGameObjectPool);
            m_DefaultGameObjectPool.Init();
        }

        public Pool AddPool(string name, Pool pool)
        {
            if (m_Name2PoolMap.ContainsKey(name)) return m_Name2PoolMap[name];
            m_Name2PoolMap.Add(name, pool);
            return pool;
        }

        public Pool GetPool(string name)
        {
            TryGetPool(name, out Pool res);
            return res;
        }

        public bool TryGetPool(string name, out Pool pool)
        {
            if (m_Name2PoolMap.ContainsKey(name))
            {
                pool = m_Name2PoolMap[name];
                return true;
            }

            pool = default;
            return false;
        }


        public void RemovePool(string name)
        {
            if (!m_Name2PoolMap.ContainsKey(name)) return;
            m_Name2PoolMap.Remove(name);
        }
    }
}