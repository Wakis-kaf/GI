using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 场景组件
    /// 负责场景的异步合同步加载
    /// 负责场景的激活与卸载
    /// </summary>
    public class SceneComponent : SingletonComponentUnit<SceneComponent>, IUnitStart, IUnitDestroy
    {
        public override string ComponentUnitName
        {
            get => "EKFScene";
        }

        [SerializeField] private EKFSceneConfig mConfig;
        private List<AsyncOperation> mAsyncLoadingList = new List<AsyncOperation>();
        private List<AsyncOperation> mAsyncLoadingTransitionList = new List<AsyncOperation>();

        private Dictionary<AsyncOperation, Action<float>> mAsyncLoadingProgress =
            new Dictionary<AsyncOperation, Action<float>>();

        private Dictionary<AsyncOperation, ISceneTransition> mAsyncLoadingTransition =
            new Dictionary<AsyncOperation, ISceneTransition>();

        private Dictionary<string, Action> mSceneLoadedCb = new Dictionary<string, Action>();
        private Dictionary<string, Action> mSceneUnLoadedCb = new Dictionary<string, Action>();

        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            // 绑定事件
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnLoaded;
            StartCoroutine(SceneAsyncLoading());
        }

        public void OnUnitStart()
        {
            LoadGameEntryScene();
        }

        private void LoadGameEntryScene()
        {
            if (mConfig.isEntryWhenFrameworkLoaded && !string.IsNullOrEmpty(mConfig.gameEntrySceneName))
            {
                // 加载场景
                if (mConfig.isAsyncLoad)
                {
                    if (mConfig.isEnableTransition
                        && mConfig.transition.gameObject != null
                        && mConfig.transition.gameObject.TryGetComponent<ISceneTransition>(out var transition))
                        LoadAsync(mConfig.gameEntrySceneName, transition).allowSceneActivation =
                            mConfig.isAllowSceneActivation;
                    else
                    {
                        LoadAsync(mConfig.gameEntrySceneName).allowSceneActivation = mConfig.isAllowSceneActivation;
                    }
                }
                else
                {
                    if (mConfig.isEnableTransition
                        && mConfig.transition.gameObject != null
                        && mConfig.transition.gameObject.TryGetComponent<ISceneTransition>(out var transition))
                        LoadSync(mConfig.gameEntrySceneName, transition);
                    else
                    {
                        LoadSync(mConfig.gameEntrySceneName);
                    }
                }
            }
        }

        public  void OnUnitDestroy()
        {
           
            // 移除事件
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnLoaded;
        }

        protected override void DisposeManagedRes()
        {
            base.DisposeManagedRes();
            for (int i = 0; i < mAsyncLoadingList.Count; i++)
            {
                mAsyncLoadingList[i].allowSceneActivation = true;
            }

            for (int i = 0; i < mAsyncLoadingTransitionList.Count; i++)
            {
                mAsyncLoadingTransitionList[i].allowSceneActivation = true;
            }
        }

        protected override void DisposeUnManagedRes()
        {
            base.DisposeUnManagedRes();
            mAsyncLoadingList.Clear();
            mAsyncLoadingTransitionList.Clear();
            mAsyncLoadingProgress.Clear();
            mAsyncLoadingTransition.Clear();
        }

        IEnumerator SceneAsyncLoading()
        {
            while (true)
            {
                for (int i = mAsyncLoadingTransitionList.Count - 1; i >= 0; i--)
                {
                    var ao = mAsyncLoadingTransitionList[i];
                    if (ao.isDone && ao.allowSceneActivation)
                    {
                        if (mAsyncLoadingTransition.ContainsKey(ao))
                        {
                            mAsyncLoadingTransition[ao].OnEnd();
                            mAsyncLoadingTransition.Remove(ao);
                        }

                        mAsyncLoadingTransitionList.RemoveAt(i);
                    }
                }

                for (int i = mAsyncLoadingList.Count - 1; i >= 0; i--)
                {
                    var ao = mAsyncLoadingList[i];
                    if (ao.isDone && ao.allowSceneActivation)
                    {
                        if (mAsyncLoadingProgress.ContainsKey(ao))
                        {
                            mAsyncLoadingProgress.Remove(ao);
                        }

                        mAsyncLoadingList.RemoveAt(i);
                    }
                }

                for (int i = 0; i < mAsyncLoadingList.Count; i++)
                {
                    var ao = mAsyncLoadingList[i];
                    float progress = ao.progress >= 0.9f ? 1 : ao.progress;
                    mAsyncLoadingProgress[ao]?.Invoke(progress);
                }

                for (int i = 0; i < mAsyncLoadingTransitionList.Count; i++)
                {
                    var ao = mAsyncLoadingTransitionList[i];
                    float progress = ao.progress >= 0.9f ? 1 : ao.progress;
                    mAsyncLoadingTransition[ao]?.OnProgressUpdate(progress);
                }

                yield return null;
            }
        }

        private void OnSceneUnLoaded(Scene scene)
        {
            if (mSceneUnLoadedCb.ContainsKey(scene.name))
            {
                mSceneUnLoadedCb[scene.name]?.Invoke();
                mSceneUnLoadedCb.Remove(scene.name);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mSceneLoadedCb.ContainsKey(scene.name))
            {
                mSceneLoadedCb[scene.name]?.Invoke();
                mSceneLoadedCb.Remove(scene.name);
            }
        }

        public AsyncOperation LoadAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            bool allowSceneActivation = true,
            Action<float> progressCB = null,
            ISceneTransition transition = null) // 异步加载场景
        {
            AsyncOperation ao = null;
            try
            {
                ao = SceneManager.LoadSceneAsync(sceneName, mode);
            }
            catch (Exception e)
            {
                Log.FatalFormat("Scene LoadAsync Error!", e.Message);
                return null;
            }


            ao.allowSceneActivation = allowSceneActivation;

            if (progressCB != null)
            {
                mAsyncLoadingList.Add(ao);
                mAsyncLoadingProgress.Add(ao, progressCB);
            }

            if (transition != null)
            {
                mAsyncLoadingTransitionList.Add(ao);
                mAsyncLoadingTransition.Add(ao, transition);
                transition.OnStart();
            }

            return ao;
        }

        public AsyncOperation LoadAsync(
            string sceneName,
            ISceneTransition transition) // 异步加载场景
        {
            return LoadAsync(sceneName, default, default, default, transition);
        }


        public AsyncOperation UnLoadSceneASync(string sceneName)
        {
            AsyncOperation ao = SceneManager.UnloadSceneAsync(sceneName);
            return ao;
        }


        public void LoadSync(
            string sceneName,
            Action cb = null,
            LoadSceneMode mode = LoadSceneMode.Single,
            ISceneTransition transition = null) // 同步加载场景
        {
            // 同步加载场景
            SceneManager.LoadScene(sceneName, mode);
            if (transition != null)
                cb += transition.OnEnd;
            if (mSceneLoadedCb.ContainsKey(sceneName))
            {
                mSceneLoadedCb[sceneName] += cb;
            }
            else
            {
                mSceneLoadedCb.Add(sceneName, cb);
            }

            transition?.OnStart();
        }

        public void LoadSync(string sceneName, ISceneTransition transition)
        {
            LoadSync(sceneName, null, default, transition);
        }

        public void LoadSync(int index, Action cb = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            string sceneName = SceneManager.GetSceneByBuildIndex(index).name;
            LoadSync(sceneName, cb, mode);
        }


        [Serializable]
        class EKFSceneConfig // 场景配置文件
        {
            public string gameEntrySceneName;
            public bool isEntryWhenFrameworkLoaded;
            public bool isAsyncLoad;
            public bool isEnableTransition;
            public bool isAllowSceneActivation = true;

            [ShowIf("@isEnableTransition == true")]
            [ValidateInput("HasISceneTransitionType", "Target must have a component implement ISceneTransition")]
            public GameObject transition;

            private bool HasISceneTransitionType(GameObject gameObject, ref string errorMessage,
                ref InfoMessageType? messageType)
            {
                if (gameObject == null) return true;

                if (gameObject.GetComponentInChildren<ISceneTransition>() == null)
                {
                    // If errorMessage is left as null, the default error message from the attribute will be used
                    errorMessage = "\"" + gameObject.name + "\" must have a component implement ISceneTransition";

                    return false;
                }

                return true;
            }
        }

        public string ControllerName { get; }
    }
}