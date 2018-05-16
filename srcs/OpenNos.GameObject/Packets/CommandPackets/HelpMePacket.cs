using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$HelpMe", PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class HelpMePacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0, SerializeToEnd = true)]
        public string Message { get; set; }

        public override string ToString() => "$HelpMe [Message]";

        #endregion
    }
}