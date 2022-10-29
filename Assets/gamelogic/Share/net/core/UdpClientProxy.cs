using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace GFramework.Network
{
    /// <summary>
    /// UDP连接管理类
    /// 异步接收消息，存入消息对象
    /// 提供发送消息接口
    /// </summary>
    public class UdpClientProxy : AChannel, IDisposable
    {
        GLogger logger = new GLogger("UdpClientProxy");

        private UdpClient udpClient;

        public UdpClientProxy(IPEndPoint iPEndPoint, ADispatcher dispatcher, IPacker packer) : base(iPEndPoint, dispatcher, packer)
        {
            this.udpClient = new UdpClient();
        }

        public void Connect(IPEndPoint iPEndPoint)
        {
            this.udpClient.Connect(iPEndPoint);
        }

        public override void Send(ProtoDefine define, byte[] msg)
        {
            byte[] data = this.packer.Pack(define, msg);
            this.udpClient.BeginSend(data, data.Length, OnSended, this.udpClient);
            logger.P($"{this.udpClient.Client.LocalEndPoint}向{this.udpClient.Client.RemoteEndPoint}发送{define}消息！！！");
        }

        public void BeginReceive()
        {
            this.udpClient.BeginReceive(OnRecevied, buffer);
        }

        private void OnSended(IAsyncResult ar)
        {
            try
            {
                int count = this.udpClient.EndSend(ar);
                logger.P($"成功发送大小为{count}的数据");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void OnRecevied(IAsyncResult ar)
        {
            try
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = this.udpClient.EndReceive(ar, ref remote);
                buffer = buffer.Concat(data).ToArray();
                bufferSize += data.Length;
                logger.P($"收到包长度：{bufferSize}");
                List<Tuple<ProtoDefine, byte[]>> protos = this.packer.UnPack(ref buffer, ref bufferSize);
                for (int i = 0; i < protos.Count; ++i)
                {
                    this.dispatcher.DecodeForm(protos[i].Item1, protos[i].Item2);
                }
                this.udpClient.BeginReceive(OnRecevied, buffer);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Dispose()
        {
            if (this.udpClient == null) return;
            this.udpClient.Close();
            this.udpClient.Dispose();
        }
    }
}
