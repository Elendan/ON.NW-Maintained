using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.HomePackets
{
    [PacketHeader("$Home", PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class HomePacket : PacketDefinition
    {
        [PacketIndex(0)]
        public string Name { get; set; }
    }
}