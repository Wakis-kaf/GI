using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace UnitFramework.Runtime
{
    // ����ض���
    public abstract class ObjectPool<T> : Pool where T : class
    {
        // ���ݲ�ͬ�ı�ǩ����Ԫ��
        private Dictionary<string, Queue<T>> m_Tag2Pool = new Dictionary<String, Queue<T>>();

        // �޸����б�
        private Dictionary<string, ObjectModifier> m_Name2Modifiers = new Dictionary<string, ObjectModifier>();


        public T[] ReleaseItem(string tag)
        {
            if(!m_Tag2Pool.ContainsKey(tag)) return new T[0];
            Queue<T> pool = m_Tag2Pool[tag];
            List<T> items = new List<T>();
            while (pool.Count!=0)
            {
                T item = default;
                // �ͷ�
                if(GetItem(tag, ref item, null))items.Add(item);
            }
            return items.ToArray();

        }
        public void DisposeItem(string tag)
        {
            if(!m_Tag2Pool.ContainsKey(tag)) return ;
            Queue<T> pool = m_Tag2Pool[tag];
            T res = default;
            while (pool.Count!=0)
            {
                var item = pool.Dequeue();
                DestroyItem(tag, item, ref res, null);
            }

        }
        public void DisposeAllItem()
        {
            var keys = m_Tag2Pool.Keys;
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys.ElementAt(i);
                DisposeItem(key);
            }
        }
        public T[] ReleaseAllItem()
        {
            // �ͷ�
            var keys = m_Tag2Pool.Keys;
            List<T> res = new List<T>();
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys.ElementAt(i);
                var items = ReleaseItem(key);
                if(items.Length>0)
                    res.AddRange(items);
            }

            return res.ToArray();
        }
        /// <summary>
        /// �ӳ���ȡ�����ߴ�������
        /// </summary>
        /// <returns></returns>
        public T GetOrCreateItem(string tag, IModifierData getData = null, IModifierData createData = null)
        {
            T res = default;
            if (!GetItem(tag, ref res, getData))
            {
                CreateItem(tag, ref res, createData);
            }

            return res;
        }

        /// <summary>
        /// ȡ������
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="res"></param>
        /// <param name="getData"></param>
        /// <returns></returns>
        public bool GetItem(string tag, ref T res, IModifierData getData)
        {
            if (tag == "") return false;
            // �ж��Ƿ��ɸñ�ǩ�Ķ��У�û���򴴽�,���ҷ���һ���µĶ���
            if (!m_Tag2Pool.ContainsKey(tag) || m_Tag2Pool[tag].Count == 0)
            {
                Log.Error($"[ObjectPool] ��ǰ������в����� Tag Ϊ{tag}�Ķ���!");
                return false;
            }

            T item = default;
            // �Ӷ������ȡ��
            item = m_Tag2Pool[tag].Dequeue();
            Log.Info($" [ObjectPool] �Ӷ������ȡ������!Tag: {tag}");
            if (m_Tag2Pool[tag].Count == 0)
            {
                m_Tag2Pool.Remove(tag);
            }
            item = ModifiersGetProcess(item, getData);
            if (item is IPoolElement poolElement)
                poolElement.OnGet();
            res = item;
            return true;
        }
        
        
        
        private T ModifiersGetProcess(T item, IModifierData data)
        {
            T res = item;
            foreach (var modifierKV in m_Name2Modifiers)
            {
                res = (T) modifierKV.Value.OnGet(res, data);
            }
            return res;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="???"></param>
        /// <param name="res"></param>
        /// <param name="createData"></param>
        /// <returns></returns>
        public bool CreateItem(string tag, ref T res, IModifierData createData)
        {
            if (tag == "") return false;
            T item = default;
            res = ModifiersCreateProcess(item, createData);
            if (res is IPoolElement poolElement)
                poolElement.OnCreate();
            return true;
        }

        private T ModifiersCreateProcess(T item, IModifierData data)
        {
            T res = item;
            foreach (var modifierKV in m_Name2Modifiers)
            {
                res = (T) modifierKV.Value.OnCreate(res, data);
            }

            return res;
        }

        /// <summary>
        /// ����������ٶ���
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public T PutOrDestroyItem(string tag, T item, IModifierData putData = default,
            IModifierData destroyData = default)
        {
            T res = default;
            if (!PutItem(tag, item, ref res, putData))
            {
                DestroyItem(tag, item, ref res, destroyData);
            }

            return res;
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        /// <param name="putData"></param>
        /// <returns></returns>
        public bool PutItem(string tag, T item, ref T res, IModifierData putData = default)
        {
            if (tag.Equals("")) return false;
            tag = tag.Trim();
            // �����������,���ҵ�ǰ���ӳ������޾��˳�
            if (!m_Tag2Pool.ContainsKey(tag) && m_Tag2Pool.Keys.Count >= tagLimitCount)
            {
                Log.Error($"[ObjectPool]��Ŷ���ʧ��!Tag: {tag}�������������!");
                return false;
            }
            else if (m_Tag2Pool.ContainsKey(tag) && m_Tag2Pool[tag].Count >= poolLimitCount) // �ж϶����Ƿ�����
            {
                Log.Info($"[ObjectPool]���ٶ���!Tag: {tag}");
                return false;
            }
            else if (!m_Tag2Pool.ContainsKey(tag))
            {
                m_Tag2Pool.Add(tag, new Queue<T>(poolInitSize));
            }

            T processRes = ModifiersPutProcess(item, putData);
           
            // ���
            m_Tag2Pool[tag].Enqueue(processRes);
            
            res = processRes;
            if (res is IPoolElement poolElement)
                poolElement.OnPut();
            Log.Info($"[ObjectPool]���!Tag: {tag},��ǰ����{m_Tag2Pool[tag].Count}");
            return true;
        }
        private T ModifiersPutProcess(T item, IModifierData data)
        {
            T res = item;
            foreach (var modifierKV in m_Name2Modifiers)
            {
                res = (T) modifierKV.Value.OnPut(res, data);
            }

            return res;
        }

        /// <summary>
        /// ������Ʒ
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        /// <param name="destroyData"></param>
        /// <returns></returns>
        public bool DestroyItem(string tag, T item, ref T res, IModifierData destroyData)
        {
            if (tag.Equals("")) return false;
            res = ModifiersDestroyProcess(item, destroyData);
            if (res is IPoolElement poolElement)
                poolElement.OnDestroy();
            return true;
        }
        private T ModifiersDestroyProcess(T item, IModifierData data)
        {
            T res = item;
            foreach (var modifierKV in m_Name2Modifiers)
            {
                res = (T) modifierKV.Value.OnDestroy(res, data);
            }

            return res;
        }
        /// <summary>
        /// Ԥ�ȶ���
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        /// <param name="count"></param>
        public void PrewarmItem(string tag, T item, int count,
            IModifierData createData = default,
            IModifierData prewarmData = default,
            IModifierData putData = default,
            IModifierData destroyData = default)
        {
            for (int i = 0; i < count; i++)
            {
                T newItem = default;
                // ��������
                CreateItem(tag, ref newItem, createData);
                // �����޸�����Ԥ�ȷ���
                newItem = ModifiersPrewarmProcess(newItem, prewarmData);
                if (newItem is IPoolElement poolElement)
                    poolElement.OnPrewarm();
                // ������������
                newItem = PutOrDestroyItem(tag, newItem, putData, destroyData);
            }
        }

        
        private T ModifiersPrewarmProcess(T item, IModifierData data)
        {
            T res = item;
            foreach (var modifierKV in m_Name2Modifiers)
            {
                res = (T) modifierKV.Value.OnPrewarm(res, data);
            }

            return res;
        }


        /// <summary>
        /// ����������޸���
        /// </summary>
        /// <param name="modifierName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TModifier CreateAndAddObjectModifier<TModifier>(string modifierName) where TModifier : ObjectModifier
        {
            if (string.IsNullOrWhiteSpace(modifierName)) return default;
            if (m_Name2Modifiers.ContainsKey(modifierName))
            {
               
                Log.Error($"[ObjectPool]�Ѿ������޸���,name{modifierName}");
                return null;
            }
            
            Log.Info($"[ObjectPool]����޸����ɹ�,name{modifierName}");
            TModifier modifier = CreateModifier<TModifier>();
            m_Name2Modifiers.Add(modifierName, modifier);
            return modifier;
        }

        /// <summary>
        /// �Ƴ��޸���
        /// </summary>
        /// <param name="modifierName"></param>
        public void RemoveObjectModifier(string modifierName)
        {
            if (string.IsNullOrWhiteSpace(modifierName)) return;
            if (!m_Name2Modifiers.ContainsKey(modifierName))
            {
                Log.Error($"[ObjectPool]�������޸���,name{modifierName}");
                return;
            }
            
            Log.Info($"[ObjectPool]�Ƴ��޸����ɹ�,name{modifierName}");
            m_Name2Modifiers.Remove(modifierName);
        }

        /// <summary>
        /// �����޸���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TModifier CreateModifier<TModifier>() where TModifier : ObjectModifier
        {
            TModifier modifier = Activator.CreateInstance<TModifier>();
            modifier.OnAwake();
            return modifier;
        }
    }
}