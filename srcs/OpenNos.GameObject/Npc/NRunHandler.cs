/*
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Packets.ClientPackets;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.GameObject.Npc
{
    public static class NRunHandler
    {
        #region Methods

        public static void NRun(ClientSession session, NRunPacket packet)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }

            MapNpc npc = session.CurrentMapInstance.Npcs.FirstOrDefault(s => s.MapNpcId == packet.NpcId);
            TeleporterDTO tp;
            var rand = new Random();
            switch (packet.Runner)
            {
				case 98:
					// Quest Easter Calvin
					if (npc == null)
					{
						return;
					}

					if (ServerManager.Instance.CalvinQuest != null)
					{
						session.Character.AddQuest((long)ServerManager.Instance.CalvinQuest);
					}
					else
					{
						session.SendPacket(
							UserInterfaceHelper.Instance.GenerateMsg(
								Language.Instance.GetMessageFromKey("ALREADY_QUEST"), 10));
						return;
					}
					break;

				case 94:
					// Quest Easter Mimi 
					if (npc == null)
					{
						return;
					}
					if (ServerManager.Instance.MimiQuest != null)
					{
						session.Character.AddQuest((long)ServerManager.Instance.MimiQuest);
					}
					else
					{
						session.SendPacket(
							UserInterfaceHelper.Instance.GenerateMsg(
								Language.Instance.GetMessageFromKey("ALREADY_QUEST"), 10));
						return;
					}
					break;

				case 96:
					if (npc == null)
					{
						return;
					}
					// 30 Rabbits vs 1 Seal Chicken king
					const short ChocolateRabbits = 2405;
					const short SealChik = 5109;
					switch (packet.Type)
					{
						case 0:
							session.SendPacket($"qna #n_run^{packet.Runner}^61^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("EXCHANGE_MATERIAL")}");
							break;
						case 61:
							if (session.Character.Inventory.CountItem(ChocolateRabbits) <= 30)
							{
								// No Lapin                  
								session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENT"), 11));
								return;
							}
							session.Character.GiftAdd(SealChik, 1);
							session.Character.Inventory.RemoveItemAmount(ChocolateRabbits, 30);
							break;
					}
					break;

				case 95:
					if (npc == null)
					{
						return;
					}
					// 5 GoldenEggs vs 1 Box
					const short GoldenEggs = 5258;
					const short BoxPascal = 5261;
					switch (packet.Type)
					{
						case 0:
							session.SendPacket($"qna #n_run^{packet.Runner}^61^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("EXCHANGE_MATERIAL")}");
							break;
						case 61:
							if (session.Character.Inventory.CountItem(GoldenEggs) <= 5)
							{
								// No GoldenEggs                   
								session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENT"), 11));
								return;
							}
							session.Character.GiftAdd(BoxPascal, 1);
							session.Character.Inventory.RemoveItemAmount(GoldenEggs, 5);
							break;
					}
					break;

				case 97:
					// Quest Easter Slugg
					if (npc == null)
					{
						return;
					}
					if (ServerManager.Instance.SluggQuest != null)
					{
						session.Character.AddQuest((long)ServerManager.Instance.SluggQuest);
					}
					else
					{
						session.SendPacket(
							UserInterfaceHelper.Instance.GenerateMsg(
								Language.Instance.GetMessageFromKey("ALREADY_QUEST"), 10));
						return;
					}
					break;

				case 99:
					// Quest Eva Easter
					if (npc == null)
					{
						return;
					}
					if (ServerManager.Instance.EvaQuest != null)
					{
						session.Character.AddQuest((long)ServerManager.Instance.EvaQuest);
					}
					else
					{
						session.SendPacket(
							UserInterfaceHelper.Instance.GenerateMsg(
								Language.Instance.GetMessageFromKey("ALREADY_QUEST"), 10));
						return;
					}
					break;

				case 100:
					// Quest Easter Malcolm
					if (npc == null)
					{
						return;
					}
					if (ServerManager.Instance.MalcolmQuest != null)
					{
						session.Character.AddQuest((long)ServerManager.Instance.MalcolmQuest);
					}
					else
					{
						session.SendPacket(
							UserInterfaceHelper.Instance.GenerateMsg(
								Language.Instance.GetMessageFromKey("ALREADY_QUEST"), 10));
						return;
					}
					break;

				case 195:
					// 5 seed Vs 2 seal Laurena
					if (npc == null)
					{
						return;
					}
					const short SeedDamnation = 5987;
					const short SealLaurena = 5977;
					switch (packet.Type)
					{
						case 0:
							session.SendPacket($"qna #n_run^{packet.Runner}^61^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("EXCHANGE_MATERIAL")}");
							break;
						case 61:
							if (session.Character.Inventory.CountItem(SeedDamnation) <= 5)
							{
								// No Seed of damnation                   
								session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENT"), 11));
								return;
							}
							session.Character.GiftAdd(SealLaurena, 2);
							session.Character.Inventory.RemoveItemAmount(SeedDamnation, 5);
							break;
					}
					break;

				case 111:
					if (npc != null)
					{
						const short gdp = 1012;
						const short Donna = 1027;
						const short Gillion = 1013;
						const short SealDraco = 5500;
						switch (packet.Type)
						{
							case 0:
								session.SendPacket($"qna #n_run^{packet.Runner}^56^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("EXCHANGE_MATERIAL")}");
								break;
							case 56:
								if (session.Character.Inventory.CountItem(Donna) <= 20 || session.Character.Inventory.CountItem(gdp) <= 20 || session.Character.Inventory.CountItem(Gillion) <= 20)
								{
									// No Material                 
									session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENT"), 11));
									return;
								}
								session.Character.GiftAdd(SealDraco, 1);
								session.Character.Inventory.RemoveItemAmount(Donna, 20);
								session.Character.Inventory.RemoveItemAmount(gdp, 20);
								session.Character.Inventory.RemoveItemAmount(Gillion, 20);
								break;
						}
					}
					break;

				case 145:
					if (npc != null)
					{
						const short Claw = 2522;
						const short SP5A = 4501;
						const short SP5E = 4500;
						const short SP5M = 4502;
						switch (packet.Type)
						{
							case 0:
								session.SendPacket($"qna #n_run^{packet.Runner}^56^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("EXCHANGE_MATERIAL")}");
								break;
							case 56:
								if (session.Character.Inventory.CountItem(Claw) <= 50)
								{
									// No Material                 
									session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENT"), 11));
									return;
								}
								switch (session.Character.Class)
								{
									case ClassType.Wrestler:
									case ClassType.Adventurer:
										return;
									case ClassType.Archer:
										session.Character.GiftAdd(SP5A, 1);
										break;
									case ClassType.Magician:
										session.Character.GiftAdd(SP5M, 1);
										break;
									case ClassType.Swordman:
										session.Character.GiftAdd(SP5E, 1);
										break;
								}
								session.Character.Inventory.RemoveItemAmount(Claw, 50);
								break;
						}
					}
					break;

				case 147:
					if (npc != null)
					{
						const short Mane = 2523;
						const short SP6A = 4498;
						const short SP6E = 4497;
						const short SP6M = 4499;
						switch (packet.Type)
						{
							case 0:
								session.SendPacket($"qna #n_run^{packet.Runner}^56^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("EXCHANGE_MATERIAL")}");
								break;
							case 56:
								if (session.Character.Inventory.CountItem(Mane) <= 50)
								{
									// No Material                 
									session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENT"), 11));
									return;
								}
								switch (session.Character.Class)
								{
									case ClassType.Wrestler:
									case ClassType.Adventurer:
										return;
									case ClassType.Archer:
										session.Character.GiftAdd(SP6A, 1);
										break;
									case ClassType.Magician:
										session.Character.GiftAdd(SP6M, 1);
										break;
									case ClassType.Swordman:
										session.Character.GiftAdd(SP6E, 1);
										break;
								}
								session.Character.Inventory.RemoveItemAmount(Mane, 50);
								break;
						}
					}
					break;

				case 148:
					if (npc != null)
					{
						const short Mane = 2523;
						const short sapphir = 2519;
						switch (packet.Type)
						{
							case 0:
								session.SendPacket($"qna #n_run^{packet.Runner}^56^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("EXCHANGE_MATERIAL")}");
								break;
							case 56:
								if (session.Character.Inventory.CountItem(Mane) <= 5)
								{
									// No Item 2523                 
									session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENT"), 11));
									return;
								}
								session.Character.GiftAdd(sapphir, 5);
								session.Character.Inventory.RemoveItemAmount(Mane, 5);
								break;
						}
					}
					break;

				case 146:
					if (npc != null)
					{
						const short Claw = 2522;
						const short Ruby = 2518;
						switch (packet.Type)
						{
							case 0:
								session.SendPacket($"qna #n_run^{packet.Runner}^56^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("EXCHANGE_MATERIAL")}");
								break;
							case 56:
								if (session.Character.Inventory.CountItem(Claw) <= 5)
								{
									// No Material                  
									session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENT"), 11));
									return;
								}
								session.Character.GiftAdd(Ruby, 5);
								session.Character.Inventory.RemoveItemAmount(Claw, 5);
								break;
						}
					}
					break;

				case 133:
					if (npc != null)
					{
						const short gdp = 1012;
						const short Icecube = 2307;
						const short Flower = 5911;
						const short SealGlagla = 5512;
						switch (packet.Type)
						{
							case 0:
								session.SendPacket($"qna #n_run^{packet.Runner}^56^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("EXCHANGE_MATERIAL")}");
								break;
							case 56:
								if (session.Character.Inventory.CountItem(Flower) <= 20 || session.Character.Inventory.CountItem(gdp) <= 20 || session.Character.Inventory.CountItem(Icecube) <= 20)
								{
									// No Material                  
									session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENT"), 11));
									return;
								}
								session.Character.GiftAdd(SealGlagla, 1);
								session.Character.Inventory.RemoveItemAmount(Flower, 20);
								session.Character.Inventory.RemoveItemAmount(gdp, 20);
								session.Character.Inventory.RemoveItemAmount(Icecube, 20);
								break;
						}
					}
					break;

				case 110:
					if (npc != null)
					{
						session.Character.AddQuest(5954);
					}
					break;

				case 131:
					if (npc != null)
					{
						session.Character.AddQuest(5982);
					}
					break;

				//quest Sp
				case 2000:
					if (npc != null)
					{
						// 932 = Pyjama
						// 933 = sp1
						// 934 = sp2
						// 948 = 3
						// 954 = 4
						// 2051 = 6
						// 2000 = quete pyj
						// 2008 = quete sp1
						// 2014 = quete sp 2
						// 2060 = quete sp3 
						// 2100 = sp4
						session.Character.AddQuest(packet.Type);
					}
					break;

				case 2001:
					// recompence Sp
					//n_run 2001 2 1 20966 = sp 1
					// n_run 2001 1 1 20966 = sp pyj
					// n_run 2001 3 1 20966 = sp 2
					// sp3 Other <=
					// sp4 Other <=
					// Pas vérifier les autre 
					break;

				case 2002:
					// Need to Add ONE Instance for 1 Rock ( With 1 npc  Rock For Quest Sp )
					ServerManager.Instance.ChangeMap(session.Character.CharacterId, 2107, 5, 11);
					break;

				case 5:
					if (npc != null)
					{
						// idk ? MAIS SA CASSE LES COUILLE QUE SA SPAM DE LA ZEUB FILS DE PUTE 
					}
					break;

				case 321:
					{
						session.Character.OpenBank();
					}
					break;
				case 322:
					{
						if (packet.Type == 0 && packet.Value == 2)
						{
							int item = session.Character.Inventory.CountItem(5836);
							if (item == 0)
							{
								Item.Item iteminfo = ServerManager.Instance.GetItem(5836);
								ItemInstance inv = session.Character.Inventory.AddNewToInventory(5836).FirstOrDefault();
							    session.SendPacket(inv != null ? "info Item Cuarry Bank Savings Book received" : UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
							}
							else
							{
								session.SendPacket($"say 1 {session.Character.CharacterId} It's already been received.");
							}
						}
					}
					break;
				case 1:
                    if (session.Character.Class != (byte)ClassType.Adventurer)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.Instance.GenerateMsg(
                                Language.Instance.GetMessageFromKey("NOT_ADVENTURER"), 0));
                        return;
                    }

                    if (session.Character.Level < 15 || session.Character.JobLevel < 20)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_LOW_LVL"),
                                0));
                        return;
                    }

                    if (packet.Type == (byte)session.Character.Class ||
                        packet.Type > 4 && session.Account.Authority < AuthorityType.GameMaster || packet.Type < 0)
                    {
                        return;
                    }

                    if (session.Character.Inventory.All(i => i.Value.Type != InventoryType.Wear))
                    {
                        session.Character.Inventory.AddNewToInventory((short)(4 + packet.Type * 14),
                            type: InventoryType.Wear);
                        session.Character.Inventory.AddNewToInventory((short)(81 + packet.Type * 13),
                            type: InventoryType.Wear);
                        switch (packet.Type)
                        {
                            case 1:
                                session.Character.Inventory.AddNewToInventory(68, type: InventoryType.Wear);
                                session.Character.Inventory.AddNewToInventory(2082, 10);
                                break;

                            case 2:
                                session.Character.Inventory.AddNewToInventory(78, type: InventoryType.Wear);
                                session.Character.Inventory.AddNewToInventory(2083, 10);
                                break;

                            case 3:
                                session.Character.Inventory.AddNewToInventory(86, type: InventoryType.Wear);
                                break;
                        }

                        foreach (ItemInstance item in session.Character.Inventory.Values.Where(i =>
                            i.Type == InventoryType.Wear && i.Item.EquipmentSlot != EquipmentType.Sp))
                        {
                            switch (item.Slot)
                            {
                                case (byte)EquipmentType.MainWeapon:
                                    session.Character.Inventory.PrimaryWeapon = (WearableInstance)item;
                                    break;
                                case (byte)EquipmentType.SecondaryWeapon:
                                    session.Character.Inventory.SecondaryWeapon = (WearableInstance)item;
                                    break;
                                case (byte)EquipmentType.Armor:
                                    session.Character.Inventory.Armor = (WearableInstance)item;
                                    break;
                            }
                        }

                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateEq());
                        session.SendPacket(session.Character.GenerateEquipment());
                        session.Character.ChangeClass((ClassType)packet.Type);
                    }
                    else
                    {
                        session.SendPacket(
                            UserInterfaceHelper.Instance.GenerateMsg(
                                Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }

                    break;

                case 2:
                    session.SendPacket("wopen 1 0");
                    break;

                case 4:
                    Mate mate = session.Character.Mates.FirstOrDefault(s => s.MateTransportId == packet.NpcId);
                    switch (packet.Type)
                    {
                        case 2:
                            if (mate != null)
                            {
                                if (session.Character.Level >= mate.Level)
                                {
                                    Mate teammate = session.Character.Mates.Where(s => s.IsTeamMember)
                                        .FirstOrDefault(s => s.MateType == mate.MateType);
                                    if (teammate != null)
                                    {
                                        teammate.RemoveTeamMember();
                                    }

                                    mate.AddTeamMember();
                                }
                                else
                                {
                                    session.SendPacket(
                                        UserInterfaceHelper.Instance.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("PET_HIGHER_LEVEL"), 0));
                                }
                            }

                            break;

                        case 3:
                            if (mate != null && session.Character.Miniland == session.Character.MapInstance)
                            {
                                if (mate.SpInstance != null || mate.IsUsingSp)
                                {
                                    session.SendPacket(session.Character.GenerateSay("partner is wearing a specialist", 10));
                                    return;
                                }
                                mate.RemoveTeamMember();
                            }

                            break;

                        case 4:
                            if (mate != null)
                            {
                                if (session.Character.Miniland == session.Character.MapInstance)
                                {
                                    if (mate.SpInstance != null || mate.IsUsingSp)
                                    {
                                        session.SendPacket(session.Character.GenerateSay("partner is wearing a specialist", 10));
                                        return;
                                    }
                                    mate.RemoveTeamMember();
                                }
                                else
                                {
                                    session.SendPacket(
                                        $"qna #n_run^4^5^3^{mate.MateTransportId} {Language.Instance.GetMessageFromKey("ASK_KICK_PET")}");
                                }
                            }

                            break;

                        case 5:
                            if (mate != null)
                            {
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(3000, 10,
                                    $"#n_run^4^6^3^{mate.MateTransportId}"));
                            }

                            break;

                        case 6:
                            if (mate != null)
                            {
                                if (session.Character.Miniland != session.Character.MapInstance)
                                {
                                    if (mate.SpInstance != null || mate.IsUsingSp)
                                    {
                                        session.SendPacket(session.Character.GenerateSay("partner is wearing a specialist", 10));
                                        return;
                                    }
                                    mate.RemoveTeamMember();
                                    session.CurrentMapInstance.Broadcast(mate.GenerateOut());
                                    session.SendPacket(session.Character.GenerateSay(
                                        string.Format(Language.Instance.GetMessageFromKey("PET_KICKED"), mate.Name),
                                        11));
                                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                                        string.Format(Language.Instance.GetMessageFromKey("PET_KICKED"), mate.Name),
                                        0));
                                }
                            }

                            break;

                        case 7:
                            if (mate != null)
                            {
                                if (session.Character.Mates.Any(s => s.MateType == mate.MateType && s.IsTeamMember))
                                {
                                    session.SendPacket(session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("ALREADY_PET_IN_TEAM"), 11));
                                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("ALREADY_PET_IN_TEAM"), 0));
                                }
                                else
                                {
                                    MateHelper.Instance.RemovePetBuffs(session);
                                    session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(3000, 10,
                                        $"#n_run^4^9^3^{mate.MateTransportId}"));
                                }
                            }

                            break;

                        case 9:
                            if (mate != null)
                            {
                                if (session.Character.Level >= mate.Level)
                                {
                                    mate.PositionX = (short)(session.Character.PositionX + 1);
                                }

                                mate.PositionY = (short)(session.Character.PositionY + 1);
                                mate.AddTeamMember();
                                session.CurrentMapInstance.Broadcast(mate.GenerateIn());
                            }
                            else
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.Instance.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("PET_HIGHER_LEVEL"), 0));
                            }

                            break;
                    }

                    session.SendPacket(session.Character.GeneratePinit());
                    session.SendPackets(session.Character.Mates.Where(s => s.IsTeamMember)
                        .OrderBy(s => s.MateType)
                        .Select(s => s.GeneratePst()));
                    break;

                case 10:
                    session.SendPacket("wopen 3 0");
                    break;

                case 12:
                    session.SendPacket($"wopen {packet.Type} 0");
                    break;

                case 14:
                    session.SendPacket("wopen 27 0");
                    string recipelist = "m_list 2";
                    if (npc != null)
                    {
                        List<Recipe> tps = npc.Recipes;
                        recipelist = tps.Where(s => s.Amount > 0)
                            .Aggregate(recipelist, (current, s) => current + $" {s.ItemVNum}");
                        recipelist += " -100";
                        session.SendPacket(recipelist);
                    }

                    break;

                case 15:
                    if (npc != null)
                    {
                        if (packet.Value == 2)
                        {
                            session.SendPacket(
                                $"qna #n_run^15^1^1^{npc.MapNpcId} {Language.Instance.GetMessageFromKey("ASK_CHANGE_SPAWNLOCATION")}");
                        }
                        else
                        {
                            switch (npc.MapId)
                            {
                                case 1:
                                    session.Character.SetRespawnPoint(1, 79, 116);
                                    break;

                                case 20:
                                    session.Character.SetRespawnPoint(20, 9, 92);
                                    break;

                                case 145:
                                    session.Character.SetRespawnPoint(145, 13, 110);
                                    break;

                                case (short)SpecialMapIdType.Lobby:
                                    session.Character.SetRespawnPoint((short)SpecialMapIdType.Lobby, 145, 91);
                                    break;
                            }

                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                                Language.Instance.GetMessageFromKey("RESPAWNLOCATION_CHANGED"), 0));
                        }
                    }

                    break;

                case 16:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        if (session.Character.Gold >= 1000 * packet.Type)
                        {
                            session.Character.Gold -= 1000 * packet.Type;
                            session.SendPacket(session.Character.GenerateGold());
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"),
                                    10));
                        }
                    }

                    break;

                case 17:
                    double currentRunningSeconds =
                        (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                    double timeSpanSinceLastPortal = currentRunningSeconds - session.Character.LastPortal;
                    if (packet.Type < 0)
                    {
                        // Packet hacking allowing duplication
                        return;
                    }

                    if (!(timeSpanSinceLastPortal >= 4) || !session.HasCurrentMapInstance ||
                        session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4Instance)
                    {
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                        return;
                    }

                    if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        session.SendPacket(
                            session.Character.GenerateSay(
                                Language.Instance.GetMessageFromKey("CANT_JOIN_ARENA_IN_RAID"), 10));
                        return;
                    }

                    if (session.Character.Gold >= 500 * (1 + packet.Type))
                    {
                        session.Character.LastPortal = currentRunningSeconds;
                        session.Character.Gold -= 500 * (1 + packet.Type);
                        session.SendPacket(session.Character.GenerateGold());
                        ServerManager.Instance.TeleportOnRandomPlaceInMap(session,
                            packet.Type == 0
                                ? ServerManager.Instance.ArenaInstance.MapInstanceId
                                : ServerManager.Instance.FamilyArenaInstance.MapInstanceId);
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                    }

                    break;

                case 18:
                    session.SendPacket(session.Character.GenerateNpcDialog(17));
                    break;

                case 26:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        if (session.Character.Gold >= 5000 * packet.Type)
                        {
                            session.Character.Gold -= 5000 * packet.Type;
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"),
                                    10));
                        }
                    }

                    break;

                case 45:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        if (session.Character.Gold >= 500)
                        {
                            session.Character.Gold -= 500;
                            session.SendPacket(session.Character.GenerateGold());
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"),
                                    10));
                        }
                    }

                    break;
                case 61:
                    if (session.Character.Inventory.CountItem(5917) <= 0 ||
                        session.Character.Inventory.CountItem(5918) <= 0 || npc == null)
                    {
                        return;
                    }

                    session.Character.GiftAdd(5922, 1);
                    session.Character.Inventory.RemoveItemAmount(5917);
                    session.Character.Inventory.RemoveItemAmount(5918);
                    break;
                case 62:
                    if (npc == null || session.Character.Inventory.CountItem(5919) <= 0)
                    {
                        return;
                    }

                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, 2536, 26, 31);
                    session.Character.Inventory.RemoveItemAmount(5919);
                    break;
                case 65:
                    if (npc == null)
                    {
                        return;
                    }

                    session.Character.AddQuest(5514);
                    break;
                case 66:
                    if (npc == null)
                    {
                        return;
                    }

                    session.Character.AddQuest(5914);
                    break;
                case 67:
                    if (npc == null)
                    {
                        return;
                    }

                    session.Character.AddQuest(5908);
                    break;
                case 68:
                    if (npc == null)
                    {
                        return;
                    }

                    session.Character.AddQuest(5919);
                    break;
                case 132:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                    }

                    break;

                case 137:
                    session.SendPacket("taw_open");
                    break;

                case 138:
                    ConcurrentBag<ArenaTeamMember> at = ServerManager.Instance.ArenaTeams.OrderBy(s => rand.Next())
                        .FirstOrDefault();
                    if (at != null)
                    {
                        ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                            at.FirstOrDefault(s => s.Session != null).Session.CurrentMapInstance.MapInstanceId, 69,
                            100);

                        ArenaTeamMember zenas = at.OrderBy(s => s.Order).FirstOrDefault(s =>
                            s.Session != null && !s.Dead && s.ArenaTeamType == ArenaTeamType.ZENAS);
                        ArenaTeamMember erenia = at.OrderBy(s => s.Order).FirstOrDefault(s =>
                            s.Session != null && !s.Dead && s.ArenaTeamType == ArenaTeamType.ERENIA);
                        session.SendPacket(erenia?.Session.Character.GenerateTaM(0));
                        session.SendPacket(erenia?.Session.Character.GenerateTaM(3));
                        session.SendPacket("taw_sv 0");
                        session.SendPacket(zenas?.Session.Character.GenerateTaP(0, true));
                        session.SendPacket(erenia?.Session.Character.GenerateTaP(2, true));
                        session.SendPacket(zenas?.Session.Character.GenerateTaFc(0));
                        session.SendPacket(erenia?.Session.Character.GenerateTaFc(1));
                    }
                    else
                    {
                        session.SendPacket(
                            UserInterfaceHelper.Instance.GenerateInfo(
                                Language.Instance.GetMessageFromKey("NO_TALENT_ARENA")));
                    }

                    break;
                case 135:
                    if (!ServerManager.Instance.StartedEvents.Contains(EventType.TALENTARENA))
                    {
                        session.SendPacket(npc?.GenerateSay(Language.Instance.GetMessageFromKey("ARENA_NOT_OPEN"), 10));
                    }
                    else
                    {
                        int tickets = 5 - session.Character.GeneralLogs.Count(s =>
                            s.LogType == "TalentArena" && s.Timestamp.Date == DateTime.Today);
                        if (ServerManager.Instance.ArenaMembers.All(s => s.Session != session) && tickets > 0)
                        {
                            if (ServerManager.Instance.IsCharacterMemberOfGroup(session.Character.CharacterId))
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.Instance.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("TALENT_ARENA_GROUP"), 0));
                                session.SendPacket(
                                    session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("TALENT_ARENA_GROUP"), 10));
                            }
                            else
                            {
                                session.SendPacket(session.Character.GenerateSay(
                                    string.Format(Language.Instance.GetMessageFromKey("ARENA_TICKET_LEFT"), tickets),
                                    10));
                                ServerManager.Instance.ArenaMembers.Add(new ArenaMember
                                {
                                    ArenaType = EventType.TALENTARENA,
                                    Session = session,
                                    GroupId = null,
                                    Time = 0
                                });
                            }
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                                Language.Instance.GetMessageFromKey("TALENT_ARENA_NO_MORE_TICKET"), 0));
                            session.SendPacket(session.Character.GenerateSay(
                                Language.Instance.GetMessageFromKey("TALENT_ARENA_NO_MORE_TICKET"), 10));
                        }
                    }

                    break;

                case 150:
                    if (npc == null || !npc.EffectActivated && ServerManager.Instance.LodTimes ||
                        session.Character.Level < ServerManager.Instance.MinLodLevel)
                    {
                        return;
                    }

                    if (session.Character?.Family == null)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NEED_FAMILY"),
                                0));
                        break;
                    }

                    if (session.Character.Family?.LandOfDeath == null)
                    {
                        session.Character.Family.LandOfDeath =
                            ServerManager.Instance.GenerateMapInstance(150, MapInstanceType.LodInstance,
                                new InstanceBag());
                    }

                    if (session.Character?.Family?.LandOfDeath != null)
                    {
                        ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                            session.Character.Family.LandOfDeath.MapInstanceId, 153, 145);
                    }

                    break;
                case 300:
                    if (npc == null)
                    {
                        return;
                    }

                    session.Character.AddQuest(6040);
                    break;
                case 301:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                    }

                    break;

                case 1600:
                    session.SendPacket(session.Character.OpenFamilyWarehouse());
                    break;

                case 1601:
                    session.SendPackets(session.Character.OpenFamilyWarehouseHist());
                    break;

                case 1602:
                    if (session.Character.Family != null && session.Character.Family.FamilyLevel >= 3 &&
                        session.Character.Family.WarehouseSize < 21)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (500000 >= session.Character.Gold)
                            {
                                session.SendPacket(
                                    session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }

                            session.Character.Family.WarehouseSize = 21;
                            session.Character.Gold -= 500000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DaoFactory.FamilyDao.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"),
                                    10));
                            session.SendPacket(
                                UserInterfaceHelper.Instance.GenerateModal(
                                    Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }

                    break;

                case 1603:
                    if (session.Character.Family != null && session.Character.Family.FamilyLevel >= 7 &&
                        session.Character.Family.WarehouseSize < 49)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (2000000 >= session.Character.Gold)
                            {
                                session.SendPacket(
                                    session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }

                            session.Character.Family.WarehouseSize = 49;
                            session.Character.Gold -= 2000000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DaoFactory.FamilyDao.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"),
                                    10));
                            session.SendPacket(
                                UserInterfaceHelper.Instance.GenerateModal(
                                    Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }

                    break;

                case 1604:
                    if (session.Character.Family != null && session.Character.Family.FamilyLevel >= 5 &&
                        session.Character.Family.MaxSize < 70)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (5000000 >= session.Character.Gold)
                            {
                                session.SendPacket(
                                    session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }

                            session.Character.Family.MaxSize = 70;
                            session.Character.Gold -= 5000000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DaoFactory.FamilyDao.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"),
                                    10));
                            session.SendPacket(
                                UserInterfaceHelper.Instance.GenerateModal(
                                    Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }

                    break;

                case 1605:
                    if (session.Character.Family != null && session.Character.Family.FamilyLevel >= 9 &&
                        session.Character.Family.MaxSize < 100)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (10000000 >= session.Character.Gold)
                            {
                                session.SendPacket(
                                    session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }

                            session.Character.Family.MaxSize = 100;
                            session.Character.Gold -= 10000000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DaoFactory.FamilyDao.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"),
                                    10));
                            session.SendPacket(
                                UserInterfaceHelper.Instance.GenerateModal(
                                    Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }

                    break;

                case 23:
                    if (packet.Type == 0)
                    {
                        if (session.Character.Group != null && session.Character.Group.CharacterCount == 3)
                        {
                            if (session.Character.Group.Characters.Any(s => s.Character.Family != null))
                            {
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(
                                    Language.Instance.GetMessageFromKey("GROUP_MEMBER_ALREADY_IN_FAMILY")));
                                return;
                            }
                        }

                        if (session.Character.Group == null || session.Character.Group.CharacterCount != 3)
                        {
                            session.SendPacket(
                                UserInterfaceHelper.Instance.GenerateInfo(
                                    Language.Instance.GetMessageFromKey("FAMILY_GROUP_NOT_FULL")));
                            return;
                        }

                        session.SendPacket(UserInterfaceHelper.Instance.GenerateInbox(
                            $"#glmk^ {14} 1 {Language.Instance.GetMessageFromKey("CREATE_FAMILY").Replace(' ', '^')}"));
                    }
                    else
                    {
                        if (session.Character.Family == null)
                        {
                            session.SendPacket(
                                UserInterfaceHelper.Instance.GenerateInfo(
                                    Language.Instance.GetMessageFromKey("NOT_IN_FAMILY")));
                            return;
                        }

                        if (session.Character.Family != null && session.Character.FamilyCharacter != null &&
                            session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                        {
                            session.SendPacket(
                                UserInterfaceHelper.Instance.GenerateInfo(
                                    Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                            return;
                        }

                        session.SendPacket($"qna #glrm^1 {Language.Instance.GetMessageFromKey("DISMISS_FAMILY")}");
                    }

                    break;

                case 60:
                    StaticBonusDTO medal = session.Character.StaticBonusList.FirstOrDefault(s =>
                        s.StaticBonusType == StaticBonusType.BazaarMedalGold ||
                        s.StaticBonusType == StaticBonusType.BazaarMedalSilver);
                    byte Medal = 0;
                    int Time = 0;
                    if (medal != null)
                    {
                        Medal = medal.StaticBonusType == StaticBonusType.BazaarMedalGold
                            ? (byte)MedalType.Gold
                            : (byte)MedalType.Silver;
                        Time = (int)(medal.DateEnd - DateTime.Now).TotalHours;
                    }

                    session.SendPacket($"wopen 32 {Medal} {Time}");
                    break;

                case 3000:
                    if (npc != null)
                    {
                    }

                    break;

                case 3006:
                    if (npc != null)
                    {
                        session.Character.AddQuest(packet.Type);
                    }

                    break;

                case 5001:
                    if (npc != null)
                    {
                        MapInstance ship = session.Character.Faction == FactionType.Angel
                            ? ServerManager.Instance.Act4ShipAngel
                            : ServerManager.Instance.Act4ShipDemon;
                        switch (session.Character.Faction)
                        {
                            case FactionType.Neutral:
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo("NEED_FACTION_ACT4"));
                                return;
                        }

                        if (3000 > session.Character.Gold)
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"),
                                    10));
                            return;
                        }

                        ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, ship.MapInstanceId,
                            ServerManager.Instance.RandomNumber(15, 25), ServerManager.Instance.RandomNumber(28, 33));
                    }

                    break;

                case 5002:
                    if (npc != null)
                    {
                        tp = npc.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                        if (tp != null)
                        {
                            session.SendPacket("it 3");
                            SerializableWorldServer connection =
                                CommunicationServiceClient.Instance.GetPreviousChannelByAccountId(session.Account
                                    .AccountId);
                            if (connection == null || session.Character == null)
                            {
                                break;
                            }

                            session.Character.MapId = tp.MapId;
                            session.Character.MapX = tp.MapX;
                            session.Character.MapY = tp.MapY;
                            session.Character.ChangeChannel(connection.EndPointIp, connection.EndPointPort, 3);
                        }
                    }

                    break;

                case 5004:
                    if (npc == null)
                    {
                        return;
                    }

                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, 145, 52, 41);
                    break;

                case 5011:
                    if (npc != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, 170, 127, 46);
                    }

                    break;

                case 5012:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                    }

                    break;

                default:
                    Logger.Log.Warn(
                        string.Format(Language.Instance.GetMessageFromKey("NO_NRUN_HANDLER"), packet.Runner));
                    break;
            }
        }

        #endregion
    }
}