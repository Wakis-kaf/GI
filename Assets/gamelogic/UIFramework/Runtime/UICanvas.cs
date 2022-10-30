using System;
using System.Collections.Generic;

using UnityEngine;

namespace UGFramework.UI
{
    /// <summary>
    /// UI层级
    /// </summary>
    [Serializable]
    public enum UILayer
    {
        Scene,
        Touch,
        Common,
        Tips,
    }

    [Serializable]
    public enum UINode
    {
        low,
        middle,
        top,
    }

    public static class UICanvas
    {
        private static Dictionary<UILayer, Dictionary<UINode, Transform>> uiMap =
            new Dictionary<UILayer, Dictionary<UINode, Transform>>();

        public static void Setup()
        {
            GameObject rootPreb = UIMgr.LoadUIAsset(UIConfig.canvasPrefab);
            Transform root = GameObject.Instantiate<GameObject>(rootPreb).transform;
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            foreach (var layer in Enum.GetValues(typeof(UILayer)))
            {
                Transform layerGO = root.Find(layer.ToString());
                Dictionary<UINode, Transform> nodeMap = new Dictionary<UINode, Transform>();
                foreach (var node in Enum.GetValues(typeof(UINode)))
                {
                    Transform nodeGO = layerGO.Find(node.ToString());
                    nodeMap[(UINode)node] = nodeGO;
                }
                uiMap[(UILayer)layer] = nodeMap;
            }
            root.gameObject.layer = LayerMask.NameToLayer("UI");
        }

        public static void SetParentOfUI(this Transform transform, UILayer layer, UINode node)
        {
            transform._SetParent(uiMap[layer][node], true);
            transform._RectTransform()?._MaxAnchors();
        }
    }
}