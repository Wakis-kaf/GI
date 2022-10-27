using System.Collections;
using UnityEngine;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 框架驱动器
    /// 框架由三部分组成： 1.驱动器 2.模块 3.组件
    /// 1.驱动器负责驱动框架进行基础的启动、轮询、销毁
    /// 2.模块负责框架运行的所需基础功能 ：1.事件模块2.扩展支持模块3.数据模型模块4.单元模块
    /// </summary>
    public class UnitFrameworkMonoDriver : MonoBehaviour
    {
        private enum FrameUpdateMode
        {
            MonoUpdate,
            MonoLateUpdate,
            MonoFixedUpdate,
            CoroutineUpdate
        }

        public static UnitFrameworkMonoDriver Instance { get; private set; }

        [SerializeField] private FrameUpdateMode frameworkUpdateMode;

        public UnitFrameworkMonoDriver()
        {
            //GameFramework.CreateInstance(); // 创建框架
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            GameFramework.CreateInstance(); // 创建框架
            DontDestroyOnLoad(gameObject);

            StartGameFramework();
            StartUpdateGameFramework();
        }

        private void StartGameFramework()
        {
            GameFramework.Instance.StartFramework();
        }

        private void StartUpdateGameFramework()
        {
            if (frameworkUpdateMode == FrameUpdateMode.CoroutineUpdate)
                StartCoroutine(UpdateCoroutine());
        }

        IEnumerator UpdateCoroutine()
        {
            while (GameFramework.Instance.IsPlaying)
            {
                yield return null;
                GameFramework.Instance.OnFrameUpdate();
            }
        }

        private void Update()
        {
            UpdateGameFramework();
        }

        private void UpdateGameFramework()
        {
            switch (frameworkUpdateMode)
            {
                case FrameUpdateMode.MonoUpdate:
                    GameFramework.Instance.OnFrameUpdate();
                    break;
            }
        }

        private void FixedUpdate()
        {
            GameFramework.Instance.OnFrameFixedUpdate();
        }

        // private void LateUpdate()
        // {
        //     if(mFrameUpdateMode == FrameUpdateMode.MonoLateUpdate)
        //         GameFrame.Instance.FrameLateUpdate();
        // }

        private void OnDestroy()
        {
            StopAllCoroutines();
            ShutdownGameFramework();
        }

        private void ShutdownGameFramework()
        {
            GameFramework.Instance.ShutdownFramework();
        }
    }
}