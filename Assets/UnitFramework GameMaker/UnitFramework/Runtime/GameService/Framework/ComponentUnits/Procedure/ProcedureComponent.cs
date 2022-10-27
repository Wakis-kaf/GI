using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitFramework.Runtime;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnitFramework.Utils;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class ProcedureComponent : SingletonComponentUnit<ProcedureComponent>, IController, IUnitStart
    {
        public override string ComponentUnitName
        {
            get => "ProcedureComponent";
        }

        public string ControllerName
        {
            get => "ProcedureController";
        }

        [SerializeField] private FSMProcessConfig m_Config;
        private List<IProcedureTransition> m_ProcedureTransitions = new List<IProcedureTransition>();
        private Procedure m_CurrentProcess;
        private IProcedureTransition m_CurrentTransition;
        private Dictionary<Type, Procedure> m_Type2ProcedureMap = new Dictionary<Type, Procedure>();

        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            LoadConfig(m_Config);
            LoadTransition();
        }


        public void LoadConfig(FSMProcessConfig processConfig)
        {
            RegisterProcedures(processConfig);
        }

        private void RegisterProcedures(FSMProcessConfig processConfig)
        {
            ReflectionProcesses();
            // 根据 CurrentProcedureTypeNames 创建状态机
            // foreach (var name in processConfig.CurrentProcedureTypeNames)
            // {
            //     Type procedureType = Type.GetType(name);
            //     if (procedureType == null) continue;
            //     Procedure procedure = ReflectionTool.CreateInstance<Procedure>(procedureType);
            //     if (!ReferenceEquals(procedure, null))
            //     {
            //         AddProcedure(procedure);
            //     }
            // }
        }

        public Procedure AddProcedure(Procedure procedure)
        {
            if (m_Type2ProcedureMap.ContainsKey(procedure.type)) return m_Type2ProcedureMap[procedure.type];
            m_Type2ProcedureMap.Add(procedure.type, procedure);
            procedure.OnLoad(this);
            Log.DebugInfo($"[Procedure Register] {procedure.type}");
            return procedure;
        }


        public void LoadTransition()
        {
            // 查找所有的ProcedureTransition
            Type[] genericProcedureTransition =
                ReflectionTool.GetSubClassOfRawGeneric(typeof(ProcedureTransition<,>), true);
            for (int i = 0; i < genericProcedureTransition.Length; i++)
            {
                var procedureTransition = genericProcedureTransition[i];
                if (!typeof(IProcedureTransition).IsAssignableFrom(procedureTransition)) continue;
                LoadTransition(procedureTransition);
            }
        }

        private void LoadTransition(Type procedureTransitionType)
        {
            IProcedureTransition transition =
                System.Activator.CreateInstance(procedureTransitionType) as IProcedureTransition;
            AddTransition(transition);
        }

        public IProcedureTransition AddTransition(IProcedureTransition transition)
        {
            if (m_ProcedureTransitions.Contains(transition)) return transition;
            Log.DebugInfo($"Register Transition {transition.GetType()}");
            m_ProcedureTransitions.Add(transition);
            m_ProcedureTransitions.Sort(ProcedureTransitionCompare);
            return transition;
        }

        private int ProcedureTransitionCompare(IProcedureTransition transition1, IProcedureTransition transition2)
        {
            return transition1.priority < transition2.priority ? 1 : -1;
        }

        public void OnUnitStart()
        {
            StartEntranceProcedure(m_Config);
            StartTransitions();
        }

        private void StartEntranceProcedure(FSMProcessConfig processConfig)
        {
            Type type = Type.GetType(processConfig.EntranceProcedureTypeName);
            if (type == null) return;
            SwitchProcess(type);
        }

        public Procedure SwitchProcess(Type type)
        {
            Procedure procedure = GetProcess(type);
            return SwitchProcess(procedure);
        }

        private Procedure SwitchProcess(Procedure procedure)
        {
            if (ReferenceEquals(procedure, null))
            {
                Log.Error($"target procedure is null {procedure.typeStr}");
                return m_CurrentProcess;
            }


            // 上一个流程退出
            if (!ReferenceEquals(m_CurrentProcess, null))
            {
                m_CurrentProcess.OnExit(this);
            }

            // 下一个流程进入
            if (procedure.hasFirstEntered)
            {
                Log.DebugInfo($"target procedure  {procedure.typeStr} first enter");
                procedure.OnFirstEnter(this);
                procedure.hasFirstEntered = false;
            }

            Log.DebugInfo($"target procedure  {procedure.typeStr} enter");
            procedure.OnEnter(this);
            m_CurrentProcess = procedure;
            return m_CurrentProcess;
        }


        private void StartTransitions()
        {
            for (int i = 0; i < m_ProcedureTransitions.Count; i++)
            {
                m_ProcedureTransitions[i].OnFrameStart();
            }
        }


        private void ReflectionProcesses()
        {
            m_Type2ProcedureMap.Clear();

            // 使用反射获取所有的Process类
            List<Procedure> processes = ReflectionTool.CreateDerivedClassInstance<Procedure>();
            foreach (var procedure in processes)
            {
                AddProcedure(procedure);
                //Debug.Log(procedure.type);
            }
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            QuitProcedures();
            ShutdownTransitions();
        }

        private void QuitProcedures()
        {
            var procedures = m_Type2ProcedureMap.Values.ToArray();
            for (int i = procedures.Length - 1; i >= 0; i--)
            {
                var value = procedures[i];
                value.OnQuit(this);
                RemoveProcedure(value);
            }
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            m_Type2ProcedureMap.Clear();
            m_Type2ProcedureMap = null;
            m_ProcedureTransitions.Clear();
            m_ProcedureTransitions = null;
        }


        /// <summary>
        /// 移除流程
        /// </summary>
        /// <param name="procedure"></param>
        public void RemoveProcedure(Procedure procedure)
        {
            if (m_Type2ProcedureMap.ContainsKey(procedure.type))
            {
                procedure.OnUnLoad(this);
                m_Type2ProcedureMap[procedure.type].Dispose();
                m_Type2ProcedureMap.Remove(procedure.type);
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

        public Procedure GetProcess<T>() where T : Procedure
        {
            Type type = typeof(T);
            if (m_Type2ProcedureMap.ContainsKey(type)) return m_Type2ProcedureMap[type];
            return default;
        }

        public Procedure GetProcess(Type type)
        {
            if (m_Type2ProcedureMap.ContainsKey(type)) return m_Type2ProcedureMap[type];
            return default;
        }

        public void ControllerUpdate()
        {
            UpdateTransitions();
            TransitionTransitingCheck();
            UpdateCurrentProcedure();
        }

        private void UpdateTransitions()
        {
            for (int i = 0; i < m_ProcedureTransitions.Count; i++)
            {
                m_ProcedureTransitions[i].OnUpdate();
            }
        }

        private void TransitionTransitingCheck()
        {
            for (int i = 0; i < m_ProcedureTransitions.Count; i++)
            {
                var transition = m_ProcedureTransitions[i];

                // check transition
                if (transition.IsTransitionReady())
                {
                    bool isTransitingSuc = TryTransiting(transition);
                    if (isTransitingSuc) m_CurrentTransition = transition;
                }
            }
        }

        private bool TryTransiting(IProcedureTransition transition)
        {
            if (transition.procedureFrom == m_CurrentProcess.type
                && transition.procedureTo != m_CurrentProcess.type
                && transition.enable)
            {
                Debug.Log($"From {transition.procedureFrom} To {transition.procedureTo} ,m_CurrentProcess.type");
                // 开始执行过渡
                return ExecutingTransition(transition);
            }

            return false;
        }

        private bool ExecutingTransition(IProcedureTransition transition)
        {
            Type to = transition.procedureTo;
            Procedure res = SwitchProcess(to);
            Log.Info(
                $"Executing Transition from {transition.procedureFrom} to {transition.procedureTo}");
            if (ReferenceEquals(res, m_CurrentProcess)) return false;
            return true;
        }

        private void UpdateCurrentProcedure()
        {
            if (!ReferenceEquals(m_CurrentProcess, null))
            {
                m_CurrentProcess.OnStay(this);
            }
        }


        private void ShutdownTransitions()
        {
            int count = m_ProcedureTransitions.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                m_ProcedureTransitions[i].OnShutdown();
            }
        }

        [DrawWithUnity]
        [System.Serializable]
        public class FSMProcessConfig
        {
            [SerializeField] private string[] _availableProcedureTypeNames = null;


            [SerializeField] private string[] _currentProcedureTypeNames = null;
            public string[] CurrentProcedureTypeNames => _currentProcedureTypeNames;

            public string[] AvailableProcedureTypeNames => _availableProcedureTypeNames;

            [SerializeField] private string _entranceProcedureTypeName = null;

            public string EntranceProcedureTypeName => _entranceProcedureTypeName;

            public void OnValidate()
            {
                if (_availableProcedureTypeNames == null || _currentProcedureTypeNames == null) return;

                string[] names = ReflectionTool.GetDerivedClassTypeName<Procedure>().ToArray();
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