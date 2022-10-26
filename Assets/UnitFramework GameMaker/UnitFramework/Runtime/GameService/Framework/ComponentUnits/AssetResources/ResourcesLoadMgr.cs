using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 负责Resources下的资源加载
    /// </summary>
    public class ResourcesLoadMgr : SingletonComponentUnit<ResourcesLoadMgr>
    {
        public override string ComponentUnitName
        {
            get => "ResourcesLoadMgr";
        }

        private HashSet<string> _resourcesList;

        public override void OnUnitAwake()
        {
            base.OnUnitAwake();

            ReadConfig();
        }

        private ResourcesLoadMgr()
        {
            _resourcesList = new HashSet<string>();
        }

        private void ReadConfig()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("FileList");
            if (textAsset == null)
            {
                Debug.LogWarning("FileList not exists ");
                return;
            }

            string txt = textAsset.text;
            //Debug.Log("content" + txt);
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