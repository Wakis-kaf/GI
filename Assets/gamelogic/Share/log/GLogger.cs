using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UNITY_STANDALONE_WIN
using UnityEngine;
#endif
namespace GFramework
{
    public class GLogger
    {
        private string owner;
        private bool logEnable = true;

        public GLogger(string owner, bool logEnable = true)
        {
            this.owner = owner;
            this.logEnable = logEnable;
        }

        public void P(object info)
        {
            string infoData = LogDumper.DumpAsString(info);
            string msg = PrintFormat($"[[{owner}]] ==> {infoData}");
            Log(msg);
        }

        public void W(object info)
        {
            string infoData = LogDumper.DumpAsString(info);
            string msg = WarningFormat($"[[{owner}]] ==> {infoData}");
            Log(msg);
        }

        public void E(object info)
        {
            string infoData = LogDumper.DumpAsString(info);
            string msg = ErrorFormat($"[[{owner}]] ==> {infoData}");
            Log(msg);
        }

        private string PrintFormat(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
#if UNITY_STANDALONE_WIN
            msg = ColorFormat("white", msg);
#endif
            return msg;
        }

        private string WarningFormat(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
#if UNITY_STANDALONE_WIN
            msg = ColorFormat("yellow", msg);
#endif
            return msg;
        }

        private string ErrorFormat(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
#if UNITY_STANDALONE_WIN
            msg = ColorFormat("red", msg);
#endif
            return msg;
        }

        private string ThrowFormat(string msg)
        {
            return $"<color=purple>{msg}</color>";
        }

        private string ColorFormat(string color, string msg)
        {
            return $"<color={color}>{msg}</color>";
        }

        private void Log(string msg)
        {
            if (!logEnable) return;
#if UNITY_STANDALONE_WIN
            Debug.Log(msg);
#else
            Console.WriteLine(msg);
#endif
        }
    }
}