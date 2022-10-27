
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnitFramework.Runtime.Network
{
    public class NetworkComponent : SingletonComponentUnit<NetworkComponent>,IController,IUnitDestroy
    {
        public override string ComponentUnitName { get=>"NetworkComponent"; }
        public string ControllerName { get=>ComponentUnitName; }

        [SerializeField] private bool isAwakeConnect;
        [SerializeField] private string serveIP = "127.0.0.1";
        [SerializeField] private int servePort = 0;
        
        [SerializeField] private bool m_IsEnablePing = true;
        [SerializeField,ShowIf("m_IsEnablePing",true)] 
        private int m_PingInterval = 30;

        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            NetManager.isUsePing = m_IsEnablePing;
            NetManager.pingInterval = m_PingInterval;
            // 连接服务器
            if (isAwakeConnect)
            {
                // 连接
                NetManager.Connect(serveIP,servePort);
            }
        }

        public void ControllerUpdate()
        {
            
            NetManager.Update();
        }

        public  void OnUnitDestroy()
        {
           
            // 关闭网络连接
            NetManager.Close();
        }
    }
}