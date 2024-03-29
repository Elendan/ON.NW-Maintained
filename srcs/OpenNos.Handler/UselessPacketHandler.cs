﻿/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core.Handling;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Packets.ClientPackets;

namespace OpenNos.Handler
{
    public class UselessPacketHandler : IPacketHandler
    {
        #region Members

        #endregion

        #region Instantiation

        public UselessPacketHandler(ClientSession session) => Session = session;

        #endregion

        #region Properties

        public ClientSession Session { get; }

        #endregion

        #region Methods

        public void ScpCts(ScpCtsPacket packet)
        {
            // idk
        }

        public void CClose(CClosePacket packet)
        {
            // idk
        }

        public void FStashEnd(FStashEndPacket packet)
        {
            // idk
        }

        public void FStashEnd(StashEndPacket packet)
        {
            // idk
        }

        public void Lbs(LbsPacket packet)
        {
            // idk
        }

        public void ShopClose(ShopClosePacket packet)
        {
            // Not needed for now.
        }

        public void Snap(SnapPacket packet)
        {
            // Not needed for now. (pictures)
        }

        #endregion
    }
}