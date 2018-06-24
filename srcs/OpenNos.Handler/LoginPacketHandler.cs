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
using System.Configuration;
using System.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Handling;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Packets.ClientPackets;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler
{
    public class LoginPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public LoginPacketHandler(ClientSession session) => _session = session;

        #endregion

        #region Methods

        public string BuildServersPacket(long accountId, int sessionId)
        {
            string channelpacket = CommunicationServiceClient.Instance.RetrieveRegisteredWorldServers(sessionId);

            if (channelpacket != null)
            {
                return channelpacket;
            }

            Logger.Log.Error("Could not retrieve Worldserver groups. Please make sure they've already been registered.");
            _session.SendPacket($"fail {string.Format(Language.Instance.GetMessageFromKey("MAINTENANCE"), DateTime.Now)}");

            return null;
        }

        /// <summary>
        ///     login packet
        /// </summary>
        /// <param name="loginPacket"></param>
        public void VerifyLogin(LoginPacket loginPacket)
        {
            if (loginPacket == null)
            {
                return;
            }

            var user = new UserDTO
            {
                Name = loginPacket.Name,
                Password = ConfigurationManager.AppSettings["UseOldCrypto"] == "true" ? EncryptionBase.Sha512(LoginEncryption.GetPassword(loginPacket.Password)).ToUpper() : loginPacket.Password
            };
            AccountDTO loadedAccount = DaoFactory.AccountDao.LoadByName(user.Name);
            if (loadedAccount != null && loadedAccount.Password.ToUpper().Equals(user.Password))
            {
                DaoFactory.AccountDao.WriteGeneralLog(loadedAccount.AccountId, _session.IpAddress, null, GeneralLogType.Connection, "LoginServer");

                //check if the account is connected
                if (!CommunicationServiceClient.Instance.IsAccountConnected(loadedAccount.AccountId))
                {
                    AuthorityType type = loadedAccount.Authority;
                    PenaltyLogDTO penalty = DaoFactory.PenaltyLogDao.LoadByAccount(loadedAccount.AccountId).FirstOrDefault(s => s.DateEnd > DateTime.Now && s.Penalty == PenaltyType.Banned);
                    if (penalty != null)
                    {
                        _session.SendPacket($"failc 7");
                    }
                    else
                    {
                        switch (type)
                        {
                            // TODO TO ENUM
                            case AuthorityType.Unconfirmed:
                            {
                                _session.SendPacket($"failc {(byte)LoginFailType.CantConnect}");
                            }
                                break;

                            case AuthorityType.Banned:
                            {
                                _session.SendPacket($"failc {(byte)LoginFailType.Banned}");
                            }
                                break;

                            case AuthorityType.Closed:
                            {
                                _session.SendPacket($"failc {(byte)LoginFailType.CantConnect}");
                            }
                                break;

                            default:
                            {
                                int newSessionId = SessionFactory.Instance.GenerateSessionId();
                                Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("CONNECTION"), user.Name, newSessionId);

                                if (CommunicationServiceClient.Instance.GetMaintenanceState() && loadedAccount.Authority <= AuthorityType.GameMaster)
                                {
                                    _session.SendPacket("failc 2");
                                    return;
                                }

                                try
                                {
                                    CommunicationServiceClient.Instance.RegisterAccountLogin(loadedAccount.AccountId, newSessionId, loadedAccount.Name);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log.Error("General Error SessionId: " + newSessionId, ex);
                                }

                                _session.SendPacket(BuildServersPacket(loadedAccount.AccountId, newSessionId));
                            }
                                break;
                        }
                    }
                }
                else
                {
                    _session.SendPacket($"failc {(byte)LoginFailType.AlreadyConnected}");
                }
            }
            else
            {
                _session.SendPacket($"failc {(byte)LoginFailType.AccountOrPasswordWrong}");
            }
        }

        #endregion
    }
}