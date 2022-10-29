using ProtoBuf;

namespace GFramework.Network
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class AckResp
    {
        public int ok = 0;

        public AckResp()
        {
        }

        public AckResp(int ok)
        {
            this.ok = ok;
        }
    }
}