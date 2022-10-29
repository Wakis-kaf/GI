using System.IO;
using System;
using System.Collections.Generic;

using GFramework;
using GFramework.UI;
using UnitFramework.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

#if XLua
[LuaCallCSharp]
#endif
/// <summary>
/// UI管理类
/// 面板层级管理
/// 进出UI栈显示隐藏面板
/// </summary>

public class UIMgr
{
    static readonly string basePath = "Prefabs/UI";

    public static GameObject LoadUIAsset(string path)
    {
        string url = Path.Combine(basePath, path);
        return GameService.Asset.LoadSync(url.PathFormat()) as GameObject;
    }

    // 单例ui
    private static Dictionary<Type, IView> singleViewMap = new Dictionary<Type, IView>();
    // 其他ui
    private static Stack<IView> mainStack = new Stack<IView>();

    /// <summary>
    /// 获取单例面板
    /// </summary>
    /// <value></value>
    public static T GetSingleView<T>() where T : IView
    {
        if (!singleViewMap.ContainsKey(typeof(T)))
        {
            throw new Exception(string.Format("The panel {0} does not exist", typeof(T)));
        }
        return (T)singleViewMap[typeof(T)];
    }

    public static void PopUI()
    {
        if (mainStack == null)
        {
            mainStack = new Stack<IView>();
            return;
        }
        if (mainStack.Count <= 0) return;
        mainStack.Pop().Close();
    }

    /// <summary>
    /// 生成一个新面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T1 NewUI<T1>() where T1 : BaseView, new()
    {
        T1 view = new T1();
        Type type = typeof(T1);
        object[] attrs = (ViewConfig[])type.GetCustomAttributes(typeof(ViewConfig), false);
        ViewConfig viewAttr = attrs.Length > 0 ? attrs[0] as ViewConfig : null;
        if (viewAttr != null)
        {
            if (viewAttr.isSingleton) singleViewMap[type] = view;
        }
        return view;
    }

    private static void LoadUI<T1>(T1 view) where T1 : BaseView, new()
    {
        string prefabPath = view.BindPath();
        GameObject uiPref = LoadUIAsset(prefabPath);
        // FIXME: 实例化方式待修改
        GameObject uiGO = GameObject.Instantiate(uiPref);
        Debug.Assert(uiGO);
        view.BindGO(uiGO);
    }

    public static T1 Instantiate<T1>(GameObject temp, Transform parent = null) where T1 : BaseView, new()
    {
        T1 view = NewUI<T1>();
        GameObject tempGO = GameObject.Instantiate(temp);
        view.BindGO(tempGO);
        if (parent != null)
            view.transform.SetParent(parent);
        return view;
    }

    /// <summary>
    /// 显示ui
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <returns></returns>
    public static T1 ShowUI<T1>() where T1 : BaseView, new()
    {
        Type type = typeof(T1);
        T1 view = null;
        if (singleViewMap.ContainsKey(type)) view = (T1)singleViewMap[type];
        if (view == null)
        {
            view = NewUI<T1>();
            LoadUI<T1>(view);
        }
        mainStack.Push(view);
        view.Show();
        return view;
    }
}