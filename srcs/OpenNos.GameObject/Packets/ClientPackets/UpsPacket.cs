using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.ClientPackets
{
    [PacketHeader("u_ps")]
    public class UpsPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public int MateTransportId { get; set; }

        [PacketIndex(1)]
        public UserType TargetType { get; set; }

        [PacketIndex(2)]
        public int TargetId { get; set; }

        [PacketIndex(3)]
        public int SkillSlot { get; set; }

        [PacketIndex(4)]
        public short MapX { get; set; }

        [PacketIndex(5)]
        public short MapY { get; set; }

        public override string ToString() => $"{MateTransportId} {TargetType} {TargetId} 1 {MapX} {MapY}";
    }
}