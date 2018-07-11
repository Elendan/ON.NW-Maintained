using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.HomePackets
{
    [PacketHeader("$SetHome", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class SetHomePacket : PacketDefinition
    {
		[PacketIndex(0)]
		public string Name { get; set; }

		public override string ToString()
		{
			return "$SetHome HOMDEID";
		}
	}
}