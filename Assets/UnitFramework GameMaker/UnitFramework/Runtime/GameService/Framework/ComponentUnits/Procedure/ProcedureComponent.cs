using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitFramework.Runtime;
using Sirenix.OdinInspector;
using UnitFramework.Utils;
using UnityEngine;

namespace  UnitFramework.Runtime
{
    public class ProcedureComponent : SingletonComponentUnit<ProcedureComponent>, IController
    {
        public override string ComponentUnitName { get=>"ProcedureComponent"; }
        Dictionary<Type,Procedure> _procedures = new Dictionary<Type, Procedure>();
        private Procedure _currentProcess;
        [SerializeField] private FSMProcessConfig m_Config;
        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            LoadConfig(m_Config);
        }

        private void ReflectionProcesses()
        {
            _procedures.Clear();
            // 使用反射获取所有的Process类
            List <Procedure> processes =  ReflectionTool.CreateDerivedClassInstance<Procedure>();
            foreach (var procedure in processes)
            {
                AddProcedure(procedure);
                //Debug.Log(procedure.type);
            }
        }

        protected override void DisposeManagedRes()
        {
            base.DisposeManagedRes();
            var procedures = _procedures.Values.ToArray();
            for (int i = procedures.Length - 1; i >= 0; i--)
            {
                var value = procedures[i];
                value.OnQuit(this);
                RemoveProcedure(value);
            }
          
        }

        protected override void DisposeUnManagedRes()
        {
            base.DisposeUnManagedRes();
            _procedures.Clear();
            _procedures = null;
        }

        /// <summary>
        /// 添加流程
        /// </summary>
        /// <param name="procedure"></param>
        /// <returns></returns>
        public Procedure AddProcedure(Procedure procedure)
        {
            if (_procedures.ContainsKey(procedure.type)) return _procedures[procedure.type];
            _procedures.Add(procedure.type,procedure);
            procedure.OnLoad(this);
            return procedure;
        }

        /// <summary>
        /// 移除流程
        /// </summary>
        /// <param name="procedure"></param>
        public void RemoveProcedure(Procedure procedure)
        {
            if (_procedures.ContainsKey(procedure.type))
            {
                procedure.OnUnLoad(this);
                _procedures[procedure.type].Dispose();
                _procedures.Remove(procedure.type);
            }
        }

        /// <summary>
        /// 切换流程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Procedure SwitchProcess<T>() where T : Procedure
        {
            // 查找流程
            Procedure procedure = GetProcess<T>();
            return SwitchProcess(procedure);
        }

        public Procedure SwitchProcess(Type type)
        {
            Procedure procedure = GetProcess(type);
            return SwitchProcess(procedure);
        }

        private Procedure SwitchProcess(Procedure procedure)
        {
            if (ReferenceEquals(procedure, null)) return _currentProcess;
            
            // 上一个流程退出
            if (!ReferenceEquals(_currentProcess, null))
            {
                _currentProcess.OnExit(this);
            }
            // 下一个流程进入
            if (procedure.first)
            {
                procedure.OnFirstEnter(this);
                procedure.first = false;
            }
            procedure.OnEnter(this);
            _currentProcess = procedure;
            return _currentProcess;
        }

        public Procedure GetProcess<T>() where T : Procedure
        {
            Type  type= typeof(T);
            if (_procedures.ContainsKey(type)) return _procedures[type];
            return default;
        }

        public Procedure GetProcess(Type type)
        {
            if (_procedures.ContainsKey(type)) return _procedures[type];
            return default;
        }



        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="processConfig"></param>
     
        public void LoadConfig(FSMProcessConfig processConfig)
        {
            /*
            Debug.Log("AvailableProcedureTypeNames Length"+processConfig.AvailableProcedureTypeNames.Length);
            Debug.Log("CurrentProcedureTypeNames Length"+processConfig.CurrentProcedureTypeNames.Length);
            Debug.Log("EntranceProcedureTypeName"+processConfig.EntranceProcedureTypeName);
            */
            
            // 根据 CurrentProcedureTypeNames 创建状态机
            foreach (var name in processConfig.CurrentProcedureTypeNames)
            {
                Type procedureType = Type.GetType(name);
                if(procedureType == null) continue;
                Procedure procedure =  ReflectionTool.CreateInstance<Procedure>(procedureType);
                
                if (!ReferenceEquals(procedure, null))
                {
                    Debug.Log($"创建流程成功{name}");
                    AddProcedure(procedure);
                }
            }

            Type type =Type.GetType(processConfig.EntranceProcedureTypeName);
            if(type == null) return;
            SwitchProcess(type);

        }

        public string ControllerName { get; }
        public void ControllerUpdate()
        {
            if (!ReferenceEquals(_currentProcess,null) )
            {
                _currentProcess.OnStay(this);
            }
        }
        [DrawWithUnity] [System.Serializable]
        public class FSMProcessConfig 
        {
            
            [SerializeField] 
            private  string[] _availableProcedureTypeNames = null;

            
            [SerializeField] private string[] _currentProcedureTypeNames = null;
            public string[] CurrentProcedureTypeNames => _currentProcedureTypeNames;

            public string[] AvailableProcedureTypeNames => _availableProcedureTypeNames;
          
            [SerializeField] 
            private string _entranceProcedureTypeName = null;

            public string EntranceProcedureTypeName => _entranceProcedureTypeName;
            public  void OnValidate()
            {
              
                if(_availableProcedureTypeNames == null|| _currentProcedureTypeNames == null) return;
               
                string[] names=  ReflectionTool.GetDerivedClassTypeName<Procedure>().ToArray();
                List<string> _cur = _currentProcedureTypeNames.ToList();
                List<string> _av = _availableProcedureTypeNames.ToList();
                for (int i = 0; i < names.Length; i++)
                {
                    string name = names[i];
                    if (!_av.Contains(name))
                    {
                        _av.Add(name);
                        _cur.Add(name);
                    }
                }

                for (int i = _av.Count - 1; i >= 0; i--)
                {
                    var name = _av[i];
                    // 该类已经被删除
                    if (!names.Contains(name))
                    {
                        _av.RemoveAt(i);
                        if (_cur.Contains(name))
                        {
                            _cur.Remove(name);
                        }
                    }
                }

                _currentProcedureTypeNames = _cur.ToArray();
                _availableProcedureTypeNames = _av.ToArray();
            }
        }
    }

}
