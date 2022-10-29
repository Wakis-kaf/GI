using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace GFramework.Network
{
    public class TcpServerProxy : AChannel, IDisposable
    {
        GLogger logger = new GLogger("TcpServerProxy");

        private TcpListener listener;

        private Dictionary<IPEndPoint, TcpClientProxy> clientProxyMap = new Dictionary<IPEndPoint, TcpClientProxy>();
        private const int maxAcceptNum = 1024;

        public TcpServerProxy(IPEndPoint iPEndPoint, ADispatcher dispatcher, IPacker packer) : base(iPEndPoint, dispatcher, packer)
        {
            this.listener = new TcpListener(iPEndPoint);
            this.listener.Start();
        }

        public void Accept()
        {
            this.listener.BeginAcceptTcpClient(OnAccepted, this.listener);
        }

        public override void Send(ProtoDefine define, byte[] msg)
        {
            TcpClientProxy[] allProxys = clientProxyMap.Values.ToArray();
            for (int i = 0; i < allProxys.Length; i++)
            {
                allProxys[i].Send(define, msg);
            }
        }

        public void SendTo(IPEndPoint iPEndPoint, ProtoDefine define, byte[] msg)
        {
            if (!clientProxyMap.ContainsKey(iPEndPoint)) return;
            clientProxyMap[iPEndPoint].Send(define, msg);
        }

        private void OnAccepted(IAsyncResult ar)
        {
            try
            {
                var client = this.listener.EndAcceptTcpClient(ar);
                var remoteIP = client.Client.RemoteEndPoint as IPEndPoint;
                logger.P($"{remoteIP}连接成功！！！");
                TcpClientProxy clientProxy = new TcpClientProxy(client, this.dispatcher, this.packer);
                clientProxy.BeginReceive();
                clientProxyMap[remoteIP] = clientProxy;
                this.listener.BeginAcceptTcpClient(OnAccepted, this.listener);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void Dispose()
        {
            if (this.listener == null) return;
            this.listener.Stop();
        }
    }
}
