using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$ClearMap", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class ClearMapPacket : PacketDefinition
    {
        #region Properties

        public override string ToString() => "ClearMap Name";

        #endregion
    }
}