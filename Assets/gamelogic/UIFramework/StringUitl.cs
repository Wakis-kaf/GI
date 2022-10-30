using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UGFramework.Util
{
    public static class StringUitl
    {
        /// <summary>
        /// 是否包含中文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasChinese(this string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        /// <summary>
        /// 是否包含数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasDigit(this string str)
        {
            return Regex.IsMatch(str, "[a-zA-Z]");
        }

        /// <summary>
        /// 是否只包含字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAlpha(this string str)
        {
            return Regex.IsMatch(str, "[a-zA-Z]");
        }

        /// <summary>
        /// 剔除中文字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RidChinese(this string str)
        {
            return Regex.Replace(str, "[\u4e00-\u9fa5]", "", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 保留字母、数字、下划线
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FetchAlpAndDigAndLine(this string str)
        {
            var res = Regex.Replace(str, @"[\W+]", "", RegexOptions.IgnoreCase);
            // 剔除中文
            return res.RidChinese();
        }

        /// <summary>
        /// Pascal命名规则
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string PascalFormat(this string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }
    }
}