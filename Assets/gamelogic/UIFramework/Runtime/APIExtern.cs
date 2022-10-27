using JetBrains.Annotations;

using UnityEngine;

/// <summary>
/// unity api类扩展
/// </summary>
public static class ApiExtern
{
    /// <summary>
    /// 获取子物体组件
    /// </summary>
    /// <param name="go"></param>
    /// <param name="path"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T _Find<T>(this GameObject go, string path)
    {
        return go.transform.Find(path).GetComponent<T>();
    }

    public static T[] _Finds<T>(this GameObject go, string path)
    {
        return go.transform.Find(path).GetComponents<T>();
    }

    /// <summary>
    /// 设置父物体
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="parent"></param>
    public static void _SetParent(this GameObject gameObject, Transform parent, bool reset = false)
    {
        gameObject.transform.SetParent(parent);
        if (reset)
            gameObject.transform._Reset();
    }

    public static void _SetParent(this Transform transform, Transform parent, bool reset = false)
    {
        transform.SetParent(parent.transform);
        if (reset)
            transform.transform._Reset();
    }

    /// <summary>
    /// Transfrom 链式扩展
    /// </summary>
    /// <param name="tf"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Transform _Position(this Transform tf, Vector3 pos)
    {
        tf.position = pos;
        return tf;
    }

    public static Transform _Roration(this Transform tf, Quaternion rotation)
    {
        tf.rotation = rotation;
        return tf;
    }

    public static Transform _LocalPosition(this Transform tf, Vector3 pos)
    {
        tf.localPosition = pos;
        return tf;
    }

    public static Transform _LocalRotation(this Transform tf, Quaternion rotation)
    {
        tf.localRotation = rotation;
        return tf;
    }

    public static Transform _LocalScale(this Transform tf, Vector3 scale)
    {
        tf.localScale = scale;
        return tf;
    }

    /// <summary>
    /// 重置变换
    /// </summary>
    /// <param name="transform"></param>
    public static void _Reset(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    [CanBeNull]
    public static RectTransform _RectTransform(this Transform transform)
    {
        return transform.GetComponent<RectTransform>();
    }

    public static void _MaxAnchors(this RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}