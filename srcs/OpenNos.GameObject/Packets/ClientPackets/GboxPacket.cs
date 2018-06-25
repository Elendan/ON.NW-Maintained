using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.ClientPackets
{
	[PacketHeader("gbox")]
	public class GboxPacket : PacketDefinition
	{
		#region Properties

		[PacketIndex(0)]
		public byte Type { get; set; }

		[PacketIndex(1)]
		public int Amount { get; set; }

		[PacketIndex(2)]
		public byte Type2 { get; set; }

		#endregion
	}
}