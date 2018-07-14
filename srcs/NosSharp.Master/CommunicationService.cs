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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Service;
using Newtonsoft.Json;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;

namespace ON.NW.Master
{
    internal class CommunicationService : ScsService, ICommunicationService
    {
        #region Methods

        public long[][] RetrieveOnlineCharacters(long characterId)
        {
            List<AccountSession> connections = MsManager.Instance.ConnectedAccounts.ToList().Where(s => s.IpAddress == MsManager.Instance.ConnectedAccounts.ToList().Find(f => f.CharacterId == characterId)?.IpAddress && s.CharacterId != 0).ToList();

            long[][] result = new long[connections.Count][];

            int i = 0;
            foreach (AccountSession acc in connections)
            {
                result[i] = new long[2];
                result[i][0] = acc.CharacterId;
                result[i][1] = acc.ConnectedWorld?.ChannelId ?? 0;
                i++;
            }
            return result;
        }

        public bool GetMaintenanceState() => MsManager.Instance.MaintenanceState;

        public void SetMaintenanceState(bool state)
        {
            MsManager.Instance.MaintenanceState = state;
        }

        public bool Authenticate(string authKey)
        {
            if (string.IsNullOrWhiteSpace(authKey) || authKey != ConfigurationManager.AppSettings["MasterAuthKey"])
            {
                return false;
            }

            MsManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
            return true;
        }

        public void Cleanup()
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            MsManager.Instance.ConnectedAccounts.Clear();
            MsManager.Instance.WorldServers.Clear();
        }

        public bool ConnectAccount(Guid worldId, long accountId, long sessionId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            AccountSession account = MsManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));
            if (account != null)
            {
                account.ConnectedWorld = MsManager.Instance.WorldServers.FirstOrDefault(w => w.Id.Equals(worldId));
            }

            return account?.ConnectedWorld != null;
        }

        public bool ConnectCharacter(Guid worldId, long characterId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            //Multiple WorldGroups not yet supported by DAOFactory
            long accountId = DaoFactory.CharacterDao.LoadById(characterId)?.AccountId ?? 0;

            AccountSession account = MsManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.ConnectedWorld?.Id.Equals(worldId) == true);
            CharacterDTO character = DaoFactory.CharacterDao.LoadById(characterId);
            if (account == null || character == null)
            {
                return false;
            }

            account.CharacterId = characterId;
            account.Character = new AccountSession.CharacterSession(character.Name, character.Level, character.Gender.ToString(), character.Class.ToString());
            foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(account.ConnectedWorld.WorldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().CharacterConnected(characterId);
            }

            Console.Title = $"MASTER SERVER - Channels :{MsManager.Instance.WorldServers.Count} - Players : {MsManager.Instance.ConnectedAccounts.Count(s => s.Character != null)}";
            return true;
        }

        public SerializableWorldServer GetAct4ChannelInfo(string worldGroup)
        {
            WorldServer act4Channel = MsManager.Instance.WorldServers.FirstOrDefault(s => s.IsAct4 && s.WorldGroup == worldGroup);

            if (act4Channel != null)
            {
                return act4Channel.Serializable;
            }

            act4Channel = MsManager.Instance.WorldServers.FirstOrDefault(s => s.WorldGroup == worldGroup);
            if (act4Channel == null)
            {
                return null;
            }

            Logger.Log.Info($"[{act4Channel.WorldGroup}] ACT4 Channel elected on ChannelId : {act4Channel.ChannelId} ");
            act4Channel.IsAct4 = true;
            //ServerManager.Instance.RestoreAct4();
            return act4Channel.Serializable;
        }

        public bool IsCrossServerLoginPermitted(long accountId, int sessionId)
        {
            return MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) &&
                MsManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.SessionId.Equals(sessionId) && s.CanSwitchChannel);
        }

        public void DisconnectAccount(long accountId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            if (MsManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.CanSwitchChannel))
            {
                return;
            }

            MsManager.Instance.ConnectedAccounts.RemoveWhere(s => s.AccountId != accountId, out ConcurrentBag<AccountSession> instanceConnectedAccounts);
            MsManager.Instance.ConnectedAccounts = instanceConnectedAccounts;
        }

        public void DisconnectCharacter(Guid worldId, long characterId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (AccountSession account in MsManager.Instance.ConnectedAccounts.Where(c => c.CharacterId.Equals(characterId) && c.ConnectedWorld.Id.Equals(worldId)))
            {
                foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(account.ConnectedWorld.WorldGroup)))
                {
                    world.ServiceClient.GetClientProxy<ICommunicationClient>().CharacterDisconnected(characterId);
                }

                if (account.CanSwitchChannel)
                {
                    continue;
                }

                account.Character = null;
                account.ConnectedWorld = null;
                Console.Title = $"MASTER SERVER - Channels :{MsManager.Instance.WorldServers.Count} - Players : {MsManager.Instance.ConnectedAccounts.Count(s => s.CharacterId != 0)}";
            }
        }

        public int? GetChannelIdByWorldId(Guid worldId)
        {
            return MsManager.Instance.WorldServers.FirstOrDefault(w => w.Id == worldId)?.ChannelId;
        }

        public bool IsAccountConnected(long accountId)
        {
            return MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) &&
                MsManager.Instance.ConnectedAccounts.Any(c => c.AccountId == accountId && c.ConnectedWorld != null);
        }

        public bool IsCharacterConnected(string worldGroup, long characterId)
        {
            return MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) &&
                MsManager.Instance.ConnectedAccounts.Any(c => c.ConnectedWorld != null && c.ConnectedWorld.WorldGroup == worldGroup && c.CharacterId == characterId);
        }

        public bool IsLoginPermitted(long accountId, long sessionId)
        {
            return MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) &&
                MsManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.SessionId.Equals(sessionId) && s.ConnectedWorld == null);
        }

        public void KickSession(long? accountId, long? sessionId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers)
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().KickSession(accountId, sessionId);
            }

            if (accountId.HasValue)
            {
                MsManager.Instance.ConnectedAccounts.RemoveWhere(s => !s.AccountId.Equals(accountId.Value), out ConcurrentBag<AccountSession> tmp);
                MsManager.Instance.ConnectedAccounts = tmp;
            }
            else if (sessionId.HasValue)
            {
                MsManager.Instance.ConnectedAccounts.RemoveWhere(s => !s.SessionId.Equals(sessionId.Value), out ConcurrentBag<AccountSession> tmp);
                MsManager.Instance.ConnectedAccounts = tmp;
            }
        }

        public void RefreshPenalty(int penaltyId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers)
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdatePenaltyLog(penaltyId);
            }

            foreach (IScsServiceClient login in MsManager.Instance.LoginServers)
            {
                login.GetClientProxy<ICommunicationClient>().UpdatePenaltyLog(penaltyId);
            }
        }

        public void RegisterAccountLogin(long accountId, long sessionId, string accountName, string ipAdress)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            MsManager.Instance.ConnectedAccounts.RemoveWhere(a => !a.AccountId.Equals(accountId), out ConcurrentBag<AccountSession> tmp);
            MsManager.Instance.ConnectedAccounts = tmp;
            MsManager.Instance.ConnectedAccounts.Add(new AccountSession(accountId, sessionId, accountName, ipAdress));
        }

        public void RegisterInternalAccountLogin(long accountId, int sessionId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            AccountSession account = MsManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));

            if (account != null)
            {
                account.CanSwitchChannel = true;
            }
        }

        public bool ConnectAccountInternal(Guid worldId, long accountId, int sessionId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            AccountSession account = MsManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));
            if (account == null)
            {
                return false;
            }

            {
                account.CanSwitchChannel = false;
                account.PreviousChannel = account.ConnectedWorld;
                account.ConnectedWorld = MsManager.Instance.WorldServers.FirstOrDefault(s => s.Id.Equals(worldId));
                if (account.ConnectedWorld != null)
                {
                    return true;
                }
            }
            return false;
        }

        public SerializableWorldServer GetPreviousChannelByAccountId(long accountId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            AccountSession account = MsManager.Instance.ConnectedAccounts.FirstOrDefault(s => s.AccountId.Equals(accountId));
            return account?.PreviousChannel?.Serializable;
        }

        public int? RegisterWorldServer(SerializableWorldServer worldServer)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            var ws = new WorldServer(worldServer.Id, new ScsTcpEndPoint(worldServer.EndPointIp, worldServer.EndPointPort), worldServer.AccountLimit, worldServer.WorldGroup)
            {
                ServiceClient = CurrentClient,
                ChannelId = Enumerable.Range(1, 30).Except(MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldServer.WorldGroup)).OrderBy(w => w.ChannelId).Select(w => w.ChannelId))
                    .First(),
                Serializable = new SerializableWorldServer(worldServer.Id, worldServer.EndPointIp, worldServer.EndPointPort, worldServer.AccountLimit, worldServer.WorldGroup),
                IsAct4 = false
            };
            MsManager.Instance.WorldServers.Add(ws);
            return ws.ChannelId;
        }

        public string RetrieveRegisteredWorldServers(long sessionId)
        {
            string lastGroup = string.Empty;
            byte worldCount = 0;
            AccountSession account = MsManager.Instance.ConnectedAccounts.FirstOrDefault(s => s.SessionId == sessionId);
            if (account == null)
            {
                return null;
            }

            string channelPacket = $"NsTeST {account.AccountName} {sessionId} ";
            foreach (WorldServer world in MsManager.Instance.WorldServers.Where(x => x.IsInvisible == false).OrderBy(w => w.WorldGroup))
            {
                if (lastGroup != world.WorldGroup)
                {
                    worldCount++;
                }

                lastGroup = world.WorldGroup;

                int currentlyConnectedAccounts = MsManager.Instance.ConnectedAccounts.Count(a => a.ConnectedWorld?.ChannelId == world.ChannelId);
                int channelcolor = (int)Math.Round((double)currentlyConnectedAccounts / world.AccountLimit * 20) + 1;

                if (world.ChannelId == 51)
                {
                    continue;
                }

                channelPacket += $"{world.Endpoint.IpAddress}:{world.Endpoint.TcpPort}:{channelcolor}:{worldCount}.{world.ChannelId}.{world.WorldGroup} ";
            }

            channelPacket += "-1:-1:-1:10000.10000.1";
            return MsManager.Instance.WorldServers.Any() ? channelPacket : null;
        }

        public string RetrieveServerStatistics(bool onlinePlayers = false)
        {
            if (onlinePlayers)
            {
                return $"{MsManager.Instance.ConnectedAccounts.Count}";
            }

            Dictionary<int, List<AccountSession.CharacterSession>> dictionary =
                MsManager.Instance.WorldServers.ToDictionary(world => world.ChannelId, world => new List<AccountSession.CharacterSession>());

            foreach (IGrouping<int, AccountSession> accountConnections in MsManager.Instance.ConnectedAccounts.Where(s => s.Character != null).GroupBy(s => s.ConnectedWorld.ChannelId))
            {
                foreach (AccountSession i in accountConnections)
                {
                    dictionary[accountConnections.Key].Add(i.Character);
                }
            }

            return JsonConvert.SerializeObject(dictionary);

        }

        public int? SendMessageToCharacter(SCSCharacterMessage message)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            WorldServer sourceWorld = MsManager.Instance.WorldServers.Find(s => s.Id.Equals(message.SourceWorldId));
            if (message?.Message == null || sourceWorld == null)
            {
                return null;
            }

            switch (message.Type)
            {
                case MessageType.Family:
                case MessageType.FamilyChat:
                    foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(sourceWorld.WorldGroup)))
                    {
                        world.ServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                    }
                    return -1;

                case MessageType.PrivateChat:
                case MessageType.Whisper:
                case MessageType.WhisperGM:
                    if (message.DestinationCharacterId.HasValue)
                    {
                        AccountSession account = MsManager.Instance.ConnectedAccounts.ToList().Find(a => a.CharacterId.Equals(message.DestinationCharacterId.Value));
                        if (account?.ConnectedWorld != null)
                        {
                            account.ConnectedWorld.ServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                            return account.ConnectedWorld.ChannelId;
                        }
                    }
                    break;

                case MessageType.Shout:
                    foreach (WorldServer world in MsManager.Instance.WorldServers)
                    {
                        world.ServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                    }
                    return -1;
            }
            return null;
        }

        public void UnregisterWorldServer(Guid worldId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            MsManager.Instance.ConnectedAccounts.RemoveWhere(a => a == null || a.ConnectedWorld?.Id.Equals(worldId) != true, out ConcurrentBag<AccountSession> tmp);
            MsManager.Instance.ConnectedAccounts = tmp;
            MsManager.Instance.WorldServers.RemoveAll(w => w.Id.Equals(worldId));
        }

        public void UpdateBazaar(string worldGroup, long bazaarItemId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdateBazaar(bazaarItemId);
            }
        }

        public void UpdateFamily(string worldGroup, long familyId, bool changeFaction)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdateFamily(familyId, changeFaction);
            }
        }

        public void Shutdown(string worldGroup)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            if (worldGroup == "*")
            {
                foreach (WorldServer world in MsManager.Instance.WorldServers)
                {
                    world.ServiceClient.GetClientProxy<ICommunicationClient>().Shutdown();
                }
            }
            else
            {
                foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
                {
                    world.ServiceClient.GetClientProxy<ICommunicationClient>().Shutdown();
                }
            }
        }

        public void UpdateRelation(string worldGroup, long relationId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MsManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdateRelation(relationId);
            }
        }

        public void PulseAccount(long accountId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            AccountSession account = MsManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId));
            if (account != null)
            {
                account.LastPulse = DateTime.Now;
            }
        }

        public void CleanupOutdatedSession()
        {
            AccountSession[] tmp = new AccountSession[MsManager.Instance.ConnectedAccounts.Count + 20];
            lock(MsManager.Instance.ConnectedAccounts)
            {
                MsManager.Instance.ConnectedAccounts.ToList().CopyTo(tmp);
            }

            foreach (AccountSession account in tmp.Where(a => a != null && a.LastPulse.AddMinutes(5) <= DateTime.Now))
            {
                KickSession(account.AccountId, null);
            }
        }

        public bool ChangeAuthority(string worldGroup, string characterName, AuthorityType authority)
        {
            CharacterDTO character = DaoFactory.CharacterDao.LoadByName(characterName);
            if (character == null)
            {
                return false;
            }

            LogVIPDTO log = DaoFactory.LogVipDao.GetLastByAccountId(character.AccountId);

            if (log == null)
            {
                log = new LogVIPDTO
                {
                    AccountId = character.AccountId,
                    Timestamp = DateTime.Now,
                    VipPack = authority.ToString()
                };
                DaoFactory.LogVipDao.InsertOrUpdate(ref log);
                // FIRST TIME VIP
            }
            else
            {
                // PRO RATA
                var newlog = new LogVIPDTO
                {
                    AccountId = character.AccountId,
                    Timestamp = log.Timestamp.Date.AddMonths(1),
                    VipPack = authority.ToString()
                };
                DaoFactory.LogVipDao.InsertOrUpdate(ref log);
            }

            if (!IsAccountConnected(character.AccountId))
            {
                AccountDTO account = DaoFactory.AccountDao.LoadById(character.AccountId);
                account.Authority = authority;
                DaoFactory.AccountDao.InsertOrUpdate(ref account);
            }
            else
            {
                AccountSession account = MsManager.Instance.ConnectedAccounts.FirstOrDefault(s => s.AccountId == character.AccountId);
                account?.ConnectedWorld.ServiceClient.GetClientProxy<ICommunicationClient>().ChangeAuthority(account.AccountId, authority);
            }

            return true;
        }

        public void SendMail(string worldGroup, MailDTO mail)
        {
            if (!IsCharacterConnected(worldGroup, mail.ReceiverId))
            {
                DaoFactory.MailDao.InsertOrUpdate(ref mail);
            }
            else
            {
                AccountSession account = MsManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.CharacterId.Equals(mail.ReceiverId));
                if (account?.ConnectedWorld == null)
                {
                    DaoFactory.MailDao.InsertOrUpdate(ref mail);
                    return;
                }

                account.ConnectedWorld.ServiceClient.GetClientProxy<ICommunicationClient>().SendMail(mail);
            }
        }

        public void SetWorldServerAsInvisible(Guid worldId)
        {
            if (!MsManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer worldServer in MsManager.Instance.WorldServers.Where(x => x.Id.Equals(worldId)))
            {
                worldServer.IsInvisible = true;
            }
        }

        #endregion
    }
}