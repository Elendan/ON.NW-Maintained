using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.HomePackets
{
    [PacketHeader("$ListHome", PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class ListHomePacket : PacketDefinition
    {

    }
}