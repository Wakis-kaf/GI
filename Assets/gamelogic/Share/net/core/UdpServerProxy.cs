using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace GFramework.Network
{
    public class UdpServerProxy : AChannel, IDisposable
    {
        GLogger logger = new GLogger("UdpServerProxy", false);

        private Dictionary<IPEndPoint, UdpClientProxy> clientProxyMap = new Dictionary<IPEndPoint, UdpClientProxy>();
        private UdpClient udpClient;

        public UdpServerProxy(IPEndPoint iPEndPoint, ADispatcher dispatcher, IPacker packer) : base(iPEndPoint, dispatcher, packer)
        {
            this.udpClient = new UdpClient(iPEndPoint);
        }

        public override void Send(ProtoDefine define, byte[] msg)
        {
            byte[] data = this.packer.Pack(define, msg);
            this.udpClient.SendAsync(data, data.Length, iPEndPoint);
        }

        public void BeginReceive()
        {
            this.udpClient.BeginReceive(OnRecevied, buffer);
        }

        private void OnRecevied(IAsyncResult ar)
        {
            try
            {
                IPEndPoint remote = null;
                byte[] data = this.udpClient.EndReceive(ar, ref remote);
                Array.Copy(data, 0, this.buffer, this.bufferSize, data.Length);
                bufferSize += data.Length;
                logger.P($"{this.udpClient.Client.LocalEndPoint}收到{remote}消息，包长度：{bufferSize}");
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
