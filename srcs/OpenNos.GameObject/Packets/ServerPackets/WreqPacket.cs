﻿using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.ServerPackets
{
    [PacketHeader("wreq")]
    public class WreqPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public byte Value { get; set; }

        [PacketIndex(1)]
        public long? Param { get; set; }

        #endregion
    }
}