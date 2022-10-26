using System;


namespace UnitFramework.Utils
{
    public static partial class Utility
    {
        public static class Json
        {
            private static Utility.Json.IJsonHelper s_JsonHelper;

            /// <summary>设置 JSON 辅助器。</summary>
            /// <param name="jsonHelper">要设置的 JSON 辅助器。</param>
            public static void SetJsonHelper(Utility.Json.IJsonHelper jsonHelper)
            {
                Utility.Json.s_JsonHelper = jsonHelper;
            }

            /// <summary>将对象序列化为 JSON 字符串。</summary>
            /// <param name="obj">要序列化的对象。</param>
            /// <returns>序列化后的 JSON 字符串。</returns>
            public static string ToJson(object obj)
            {
                if (Utility.Json.s_JsonHelper == null)
                    throw new UnitFrameworkException("JSON helper is invalid.");
                try
                {
                    return Utility.Json.s_JsonHelper.ToJson(obj);
                }
                catch (Exception ex)
                {
                    if (!(ex is UnitFrameworkException))
                        throw new UnitFrameworkException(
                            string.Format("Can not convert to JSON with exception '{0}'.",
                                (object) ex.ToString()), ex);
                    throw;
                }
            }

            /// <summary>将 JSON 字符串反序列化为对象。</summary>
            /// <typeparam name="T">对象类型。</typeparam>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            public static T ToObject<T>(string json)
            {
                if (Utility.Json.s_JsonHelper == null)
                    throw new UnitFrameworkException("JSON helper is invalid.");
                try
                {
                    return Utility.Json.s_JsonHelper.ToObject<T>(json);
                }
                catch (Exception ex)
                {
                    if (!(ex is UnitFrameworkException))
                        throw new UnitFrameworkException(
                            string.Format("Can not convert to object with exception '{0}'.",
                                (object) ex.ToString()), ex);
                    throw;
                }
            }

            /// <summary>将 JSON 字符串反序列化为对象。</summary>
            /// <param name="objectType">对象类型。</param>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            public static object ToObject(Type objectType, string json)
            {
                if (Utility.Json.s_JsonHelper == null)
                    throw new UnitFrameworkException("JSON helper is invalid.");
                if (objectType == null)
                    throw new UnitFrameworkException("Object type is invalid.");
                try
                {
                    return Utility.Json.s_JsonHelper.ToObject(objectType, json);
                }
                catch (Exception ex)
                {
                    if (!(ex is UnitFrameworkException))
                        throw new UnitFrameworkException(
                            string.Format("Can not convert to object with exception '{0}'.",
                                (object) ex.ToString()), ex);
                    throw;
                }
            }

            /// <summary>JSON 辅助器接口。</summary>
            public interface IJsonHelper
            {
                /// <summary>将对象序列化为 JSON 字符串。</summary>
                /// <param name="obj">要序列化的对象。</param>
                /// <returns>序列化后的 JSON 字符串。</returns>
                string ToJson(object obj);

                /// <summary>将 JSON 字符串反序列化为对象。</summary>
                /// <typeparam name="T">对象类型。</typeparam>
                /// <param name="json">要反序列化的 JSON 字符串。</param>
                /// <returns>反序列化后的对象。</returns>
                T ToObject<T>(string json);

                /// <summary>将 JSON 字符串反序列化为对象。</summary>
                /// <param name="objectType">对象类型。</param>
                /// <param name="json">要反序列化的 JSON 字符串。</param>
                /// <returns>反序列化后的对象。</returns>
                object ToObject(Type objectType, string json);
            }
        }
    }
}