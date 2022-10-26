using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnitFramework;
using UnitFramework.Runtime;
using UnitFramework.Utils;
using LitJson;
using UnityEngine;

public class JsonLoader : SingletonComponentUnit<JsonLoader>,Utility.Json.IJsonHelper
{
    public override string ComponentUnitName
    {
        get => "JsonLoader";
    }

    public string jsonFileSaveStreamingPath = "/JsonFiles/";
    public override void OnUnitAwake()
    {
        base.OnUnitAwake();
        Utility.Json.SetJsonHelper(this);
    }

    /// <summary>
    /// 从指定路径读取json内容
    /// </summary>
    /// <param name="path"></param>
    /// <param name="jsonName"></param>
    /// <returns></returns>
    private string ReadJsonStrFromPath(string path, string jsonName)
    {
        // json name validate
        if (!jsonName.EndsWith(".json"))
        {
            jsonName += ".json";
        }

        return FileComponent.ReadFile(path, jsonName);
    }

    /// <summary>
    /// 读取Json文件并转换为目标对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="jsonName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T ReadJsonObject<T>(string path, string jsonName)
    {
        string jsonStr = ReadJsonStrFromPath(path, jsonName);
        return JsonMapper.ToObject<T>(jsonStr);
    }
    public T ReadJsonObject<T>( string jsonName)
    {
       
        string jsonStr = ReadJsonStrFromPath(GetFilePath(), jsonName);
        return JsonMapper.ToObject<T>(jsonStr);
    }

    private string GetFilePath()
    {
        string customPath = jsonFileSaveStreamingPath;
        if (!customPath.StartsWith("/"))
        {
            customPath = "/" + customPath;
        }
        string path = Utility.Path.GetPlatformStreamingPath() +customPath;
        return path;
    }
    /// <summary>
    /// 从指定路径读取json内容并转换为JsonData
    /// </summary>
    /// <param name="path"></param>
    /// <param name="jsonName"></param>
    /// <returns></returns>
    public JsonData ReadJsonFromPath(string path, string jsonName)
    {
        string jsonStr = ReadJsonStrFromPath(path, jsonName);
        if (string.IsNullOrEmpty(jsonStr)) return null;
        JsonData jsonData = JsonMapper.ToObject(jsonStr);
        return jsonData;
    }
 
    
    /// <summary>
    /// 将Json字符串为目标对象
    /// </summary>
    /// <param name="jsonStr"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T JsonToObject<T>(string jsonStr)
    {
        return JsonMapper.ToObject<T>(jsonStr);
    }
    /// <summary>
    /// 将Json字符串为目标对象
    /// </summary>
    /// <param name="jsonStr"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private object JsonToObject(string jsonStr, Type type)
    {
        //JsonMapper.ToObject(jsonStr)
        MethodInfo method = typeof(JsonMapper).GetMethod("ReadValue", BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new Type[] {typeof(Type), typeof(string)},
            null);
        return  method?.Invoke(this,new object[]{type,new JsonReader(jsonStr)});
    }
    /// <summary>
    /// 将目标对象转化为Json 字符串
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public string ObjectToJson(object obj)
    {
        return JsonMapper.ToJson(obj);
    }

    /// <summary>
    /// 保存目标对象到本地文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path"></param>
    /// <param name="jsonName"></param>
    public void SaveObjectToJsonFile(object obj,string path, string jsonName)
    {
        string content = ObjectToJson(obj);
        if (!jsonName.EndsWith(".json"))
        {
            jsonName += ".json";
        }
        FileComponent.SaveFile(path,jsonName,content);
    }
    public void SaveObjectToJsonFile(object obj, string jsonName)
    {
        
        string content = ObjectToJson(obj);
        if (!jsonName.EndsWith(".json"))
        {
            jsonName += ".json";
        }
        FileComponent.SaveFile(GetFilePath(),jsonName,content);
    }

    public string ToJson(object obj)
    {
        return ObjectToJson(obj);
    }

    public T ToObject<T>(string json)
    {
        return JsonToObject<T>(json);
    }

    public object ToObject(Type objectType, string json)
    {
        return JsonToObject(json,objectType);
    }
}