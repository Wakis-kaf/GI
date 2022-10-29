using System.Net;
namespace GFramework.Network
{
    public abstract class AChannel
    {
        public readonly IPEndPoint iPEndPoint;
        public readonly ADispatcher dispatcher;
        public readonly IPacker packer;
        protected byte[] buffer;
        protected int bufferSize = 0;
        protected const int maxBufferSize = 2048;

        public AChannel(IPEndPoint iPEndPoint, ADispatcher dispatcher, IPacker packer)
        {
            this.iPEndPoint = iPEndPoint;
            this.packer = packer;
            this.dispatcher = dispatcher;
            this.dispatcher.channel = this;
            this.buffer = new byte[maxBufferSize];
            this.bufferSize = 0;
        }

        public abstract void Send(ProtoDefine define, byte[] data);
    }
}