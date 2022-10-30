using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UGFramework.Network
{
    /// <summary>
    /// TCP连接管理类
    /// 负责与服务器建立连接保存socket对象
    /// 异步接收消息，存入消息对象
    /// 提供发送消息接口
    /// </summary>
    public class TcpClientProxy : AChannel, IDisposable
    {
        GLogger logger = new GLogger("TcpClientProxy");

        private TcpClient tcpClient;

        public TcpClientProxy(TcpClient tcpClient, ADispatcher dispatcher, IPacker packer) : base(tcpClient.Client.RemoteEndPoint as IPEndPoint, dispatcher, packer)
        {
            this.tcpClient = tcpClient;
        }

        // 连接到服务器
        public void Connect(IPEndPoint remoteIP)
        {
            logger.P($"尝试以{this.tcpClient.Client.LocalEndPoint} 连接服务器{remoteIP}...");
            this.tcpClient.BeginConnect(remoteIP.Address, remoteIP.Port, OnConnected, this.tcpClient);
        }

        public override void Send(ProtoDefine define, byte[] msg)
        {
            if (this.tcpClient.Connected)
            {
                byte[] data = this.packer.Pack(define, msg);
                this.tcpClient.GetStream().BeginWrite(data, 0, data.Length, OnSended, data);
            }
            else
            {
                logger.E("网络未连接！！！");
            }
        }

        public void BeginReceive()
        {
            if (this.tcpClient.Connected)
            {
                this.tcpClient.GetStream().BeginRead(buffer, 0, maxBufferSize, OnReceived, buffer);
                logger.P($"开始接收{this.iPEndPoint}的消息...");
            }
            else
            {
                logger.E("网络未连接！！！");
            }
        }

        /// <summary>
        /// 连接到服务器回调
        /// </summary>
        /// <param name="ar"></param>
        /// <exception cref="Exception"></exception>
        private void OnConnected(IAsyncResult ar)
        {
            try
            {
                this.tcpClient.EndConnect(ar);
                if (this.tcpClient.Connected)
                {
                    logger.P($"端口{this.tcpClient.Client.LocalEndPoint}连接{this.tcpClient.Client.RemoteEndPoint}成功！！！");
                }
                else
                {
                    logger.P("连接失败！！！");
                }
                this.BeginReceive();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void OnSended(IAsyncResult ar)
        {
            try
            {
                this.tcpClient.GetStream().EndWrite(ar);
                logger.P($"向{this.tcpClient.Client.RemoteEndPoint}发送数据成功！！！");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // 接收到服务器消息回调
        protected void OnReceived(IAsyncResult ar)
        {
            try
            {
                if (!this.tcpClient.Connected) return;
                int bufferSize = this.tcpClient.GetStream().EndRead(ar);
                var remote = this.tcpClient.Client.RemoteEndPoint;
                if (bufferSize <= 0)
                {
                    logger.P($"与{remote}断开连接！！！");
                    this.Dispose();
                    return;
                }
                logger.P($"收到{remote}>>{bufferSize}字节数据！！！");

                List<Tuple<ProtoDefine, byte[]>> protos = this.packer.UnPack(ref buffer, ref bufferSize);
                for (int i = 0; i < protos.Count; ++i)
                {
                    this.dispatcher.DecodeForm(protos[i].Item1, protos[i].Item2);
                }

                this.tcpClient.GetStream().BeginRead(buffer, bufferSize, maxBufferSize, OnReceived, buffer);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Dispose()
        {
            if (this.tcpClient == null)
                return;
            this.tcpClient.Close();
        }
    }
}
