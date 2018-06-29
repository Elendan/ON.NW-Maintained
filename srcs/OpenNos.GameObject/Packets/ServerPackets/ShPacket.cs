using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.ServerPackets
{
    [PacketHeader("sh")]
    public class ShPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public UserType TargetType { get; set; } //Not sure, need to verify

        [PacketIndex(1)]
        public int TargetId { get; set; }
    }
}
