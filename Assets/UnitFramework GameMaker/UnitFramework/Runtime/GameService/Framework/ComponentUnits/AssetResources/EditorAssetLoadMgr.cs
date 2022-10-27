using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace  UnitFramework.Runtime
{
    /// <summary>
    /// 负责 Editor 模式下资源的 加载 
    /// </summary>
    public class EditorAssetLoadMgr : SingletonComponentUnit<EditorAssetLoadMgr>
    {
        public override string ComponentUnitName { get=>"EditorAssetLoadMgr"; }

        private HashSet<string> _resourcesList;

        public override void OnUnitAwake()
        {
#if !UNITY_EDITOR
            Destroy(gameObject);
            return;
#endif
            
            base.OnUnitAwake();
            ReadConfig();
        }

        private EditorAssetLoadMgr()
        {
            _resourcesList = new HashSet<string>();
        }

        private void ReadConfig()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("EditorFileList");
            if (textAsset == null)
            {
                Debug.LogWarning("FileList not exists ");
                return;
            }

            string txt = textAsset.text;
            Debug.Log("content" + txt);
            txt = txt.Replace("\r\n", "\n");

            foreach (var line in txt.Split('\n'))
            {
                if (string.IsNullOrEmpty(line)) continue;

                if (!_resourcesList.Contains(line))
                    _resourcesList.Add(line);
            }
        }

        public bool IsFileExist(string _assetName)
        {
            return _resourcesList.Contains(_assetName);
        }

        public ResourceRequest LoadAsync(string _assetName)
        {
            if (!_resourcesList.Contains(_assetName))
            {
                Debug.LogError("ResourcesLoadMgr No Find File " + _assetName);
                return null;
            }

            ResourceRequest request = Resources.LoadAsync(_assetName);

            return request;
        }

        public UnityEngine.Object LoadSync(string _assetName)
        {
            if (!_resourcesList.Contains(_assetName))
            {
                Debug.LogError("ResourcesLoadMgr No Find File " + _assetName);
                return null;
            }

            UnityEngine.Object asset = Resources.Load(_assetName);

            return asset;
        }

        public void Unload(UnityEngine.Object asset)
        {
            if (asset is GameObject)
            {
                return;
            }

            Resources.UnloadAsset(asset);
            asset = null;
        }
    }

}
