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

using System.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Item
{
    public class TeacherItem : Item
    {
        #region Instantiation

        public TeacherItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0,
            string[] packetsplit = null)
        {
            if (packetsplit == null)
            {
                return;
            }

            int x1;
            switch (Effect)
            {
                case 11:
                case 12:
                    if (int.TryParse(packetsplit[3], out x1))
                    {
                        Mate mate = session.Character.Mates.FirstOrDefault(s => s.MateTransportId == x1);
                        if (mate == null || mate.Level >= session.Character.Level - 5)
                        {
                            return;
                        }

                        mate.Level++;
                        mate.BattleEntity.Level = mate.Level;
                        session.Character.Inventory.RemoveItemAmount(inv.ItemVNum);
                        session.CurrentMapInstance?.Broadcast(mate.GenerateEff(8), mate.PositionX, mate.PositionY);
                        session.CurrentMapInstance?.Broadcast(mate.GenerateEff(198), mate.PositionX, mate.PositionY);
                    }
                    break;

                case 13:
                    if (int.TryParse(packetsplit[3], out x1))
                    {
                        if (session.Character.Mates.Any(s => s.MateTransportId == x1))
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(10, 1, x1, 2));
                        }
                    }

                    break;

                case 14:
                    if (int.TryParse(packetsplit[3], out x1))
                    {
                        Mate mate = session.Character.Mates.FirstOrDefault(s =>
                            s.MateTransportId == x1 && s.MateType == MateType.Pet);
                        if (mate != null)
                        {
                            if (!mate.CanPickUp)
                            {
                                session.Character.Inventory.RemoveItemAmount(inv.ItemVNum);
                                session.CurrentMapInstance.Broadcast(mate.GenerateEff(5));
                                session.CurrentMapInstance.Broadcast(mate.GenerateEff(5002));
                                mate.CanPickUp = true;
                                session.SendPackets(session.Character.GenerateScP());
                                session.SendPacket(
                                    session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("PET_CAN_PICK_UP"), 10));
                            }
                        }
                    }

                    break;

                case 17:
                    if (int.TryParse(packetsplit[3], out x1))
                    {
                        Mate mate = session.Character.Mates.FirstOrDefault(s => s.MateTransportId == x1);
                        if (mate != null)
                        {
                            if (!mate.IsSummonable)
                            {
                                session.Character.Inventory.RemoveItemAmount(inv.ItemVNum, 1);
                                mate.IsSummonable = true;
                                session.SendPackets(session.Character.GenerateScP());
                                session.SendPacket(session.Character.GenerateSay(
                                    string.Format(Language.Instance.GetMessageFromKey("PET_SUMMONABLE"), mate.Name),
                                    10));
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                                    string.Format(Language.Instance.GetMessageFromKey("PET_SUMMONABLE"), mate.Name),
                                    0));
                            }
                        }
                    }

                    break;

                case 1000:
                    if (int.TryParse(packetsplit[3], out x1))
                    {
                        Mate mate = session.Character.Mates.FirstOrDefault(s =>
                            s.MateTransportId == x1 && s.MateType == MateType.Pet);
                        if (mate != null)
                        {
                            if (!mate.IsTeamMember)
                            {
                                session.Character.Mates.Remove(mate);
                                session.SendPacket(
                                    UserInterfaceHelper.Instance.GenerateInfo(
                                        Language.Instance.GetMessageFromKey("PET_RELEASED")));
                                session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());
                                session.SendPackets(session.Character.GenerateScP());
                                session.SendPackets(session.Character.GenerateScN());
                                session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                                session.CurrentMapInstance?.Broadcast(mate.GenerateOut());
                            }
                            else
                            {
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("PET_IN_TEAM_UNRELEASABLE"), 0));
                            }
                        }
                    }

                    break;

                case 1001:
                    if (int.TryParse(packetsplit[3], out x1))
                    {
                        Mate mate = session.Character.Mates.FirstOrDefault(s =>
                            s.MateTransportId == x1 && s.MateType == MateType.Partner);
                        if (mate != null)
                        {
                            if (!mate.IsTeamMember &&
                                mate.SpInstance == null &&
                                mate.WeaponInstance == null &&
                                mate.ArmorInstance == null &&
                                mate.GlovesInstance == null &&
                                mate.BootsInstance == null)
                            {
                                session.Character.Mates.Remove(mate);
                                session.SendPacket(
                                    UserInterfaceHelper.Instance.GenerateInfo(
                                        Language.Instance.GetMessageFromKey("PET_RELEASED")));
                                session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());
                                session.SendPackets(session.Character.GenerateScP());
                                session.SendPackets(session.Character.GenerateScN());
                                session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                                session.CurrentMapInstance?.Broadcast(mate.GenerateOut());
                            }
                            else
                            {
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("PET_IN_TEAM_UNRELEASABLE"), 0));
                            }
                        }
                    }

                    break;

                case 10000:
                    if (session.Character.MapInstance != session.Character.Miniland)
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("not in minimland", 1));
                        return;
                    }
                    var monster = new MapMonster
                    {
                        MonsterVNum = (short)EffectValue,
                        MapY = session.Character.PositionY,
                        MapX = session.Character.PositionX,
                        MapId = session.Character.MapInstance.Map.MapId,
                        Position = (byte)session.Character.Direction,
                        IsMoving = true,
                        IsHostile = true,
                        MapMonsterId = session.CurrentMapInstance.GetNextId(),
                        IsMateTrainer = true,
                        ShouldRespawn = false
                    };
                    monster.Initialize(session.CurrentMapInstance);
                    session.CurrentMapInstance.AddMonster(monster);
                    session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                    session.Character.Inventory.RemoveItemAmount(inv.ItemVNum);
                    break;

                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType()));
                    break;
            }
        }

        #endregion
    }
}