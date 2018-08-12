using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$Bot", PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class BotPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public short Identificator { get; set; }
    }
}