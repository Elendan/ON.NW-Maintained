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
using System.Reactive.Linq;
using System.Reflection;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Handling;
using OpenNos.Core.Networking;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.GameObject.Map;
using OpenNos.Master.Library.Client;

namespace OpenNos.GameObject.Networking
{
    public class ClientSession
    {
        #region Instantiation

        public ClientSession(INetworkClient client)
        {
            // set last received
            _lastPacketReceive = DateTime.Now.Ticks;

            // lag mode
            new Random((int)client.ClientId);

            // initialize lagging mode
            bool isLagMode = ConfigurationManager.AppSettings["LagMode"].ToLower() == "true";

            // initialize network client
            _client = client;

            // absolutely new instantiated Client has no SessionId
            SessionId = 0;

            // register for NetworkClient events
            _client.MessageReceived += OnNetworkClientMessageReceived;

            // start observer for receiving packets
            _receiveQueue = new ConcurrentQueue<byte[]>();
            Observable.Interval(new TimeSpan(0, 0, 0, 0, isLagMode ? 1000 : 10)).Subscribe(x => HandlePackets());
        }

        #endregion

        #region Members

        private static EncryptionBase _encryptor;
        private Character _character;
        private readonly INetworkClient _client;
        private IDictionary<string, HandlerMethodReference> _handlerMethods;
        private readonly ConcurrentQueue<byte[]> _receiveQueue;
        private readonly IList<string> _waitForPacketList = new List<string>();

        // Packetwait Packets
        private int? _waitForPacketsAmount;

        // private byte countPacketReceived;
        private long _lastPacketReceive;

        #endregion

        #region Properties

        public Account Account { get; private set; }

        public Character Character
        {
            get
            {
                if (_character == null || !HasSelectedCharacter)
                {
                    // cant access an
                    Logger.Log.Warn("Uninitialized Character cannot be accessed.");
                }

                return _character;
            }

            private set => _character = value;
        }

        public long ClientId => _client.ClientId;

        public MapInstance CurrentMapInstance { get; set; }

        public IDictionary<string, HandlerMethodReference> HandlerMethods
        {
            get => _handlerMethods ?? (_handlerMethods = new Dictionary<string, HandlerMethodReference>());

            set => _handlerMethods = value;
        }

        public bool HasCurrentMapInstance => CurrentMapInstance != null;

        public bool HasSelectedCharacter { get; set; }

        public bool HasSession => _client != null;

        public string IpAddress => _client.IpAddress.Contains("tcp://")
            ? _client.IpAddress.Replace("tcp://", "")
            : _client.IpAddress;

        public bool IsAuthenticated { get; set; }

        public bool IsConnected => _client.IsConnected;

        public bool IsDisposing
        {
            get => _client.IsDisposing;

            set => _client.IsDisposing = value;
        }

        public bool IsOnMap => CurrentMapInstance != null;

        public int LastKeepAliveIdentity { get; set; }

        public DateTime RegisterTime { get; internal set; }

        public int SessionId { get; set; }

        #endregion

        #region Methods

        public void ClearLowPriorityQueue()
        {
            _client.ClearLowPriorityQueue();
        }

        public void Destroy()
        {
            // unregister from events
            CommunicationServiceClient.Instance.CharacterConnectedEvent -= OnOtherCharacterConnected;
            CommunicationServiceClient.Instance.CharacterDisconnectedEvent -= OnOtherCharacterDisconnected;

            // do everything necessary before removing client, DB save, Whatever
            if (HasSelectedCharacter)
            {
                Character.Dispose();
                if (Character.MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance ||
                    Character.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                {
                    Character.MapInstance.InstanceBag.DeadList.Add(Character.CharacterId);
                    if (Character.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        Character?.Group?.Characters.ToList().ForEach(s =>
                        {
                            s.SendPacket(s.Character?.Group?.GeneraterRaidmbf(s.CurrentMapInstance));
                            s.SendPacket(s.Character?.Group?.GenerateRdlst());
                        });
                    }
                }

                if (Character?.Miniland != null)
                {
                    ServerManager.Instance.RemoveMapInstance(Character.Miniland.MapInstanceId);
                }

                // TODO Check why ExchangeInfo.TargetCharacterId is null Character.CloseTrade();
                // disconnect client
                CommunicationServiceClient.Instance.DisconnectCharacter(ServerManager.Instance.WorldId,
                    Character.CharacterId);

                // unregister from map if registered
                if (CurrentMapInstance != null)
                {
                    CurrentMapInstance.UnregisterSession(Character.CharacterId);
                    CurrentMapInstance = null;
                    ServerManager.Instance.UnregisterSession(Character.CharacterId);
                }
            }

            if (Account != null)
            {
                CommunicationServiceClient.Instance.DisconnectAccount(Account.AccountId);
            }

            ClearReceiveQueue();
        }

        public void Disconnect()
        {
            Character?.AntiBotMessageInterval?.Dispose();
            Character?.AntiBotObservable?.Dispose();
            Character?.SaveObs?.Dispose();
            _client.Disconnect();
        }

        public void Initialize(EncryptionBase encryptor, Type packetHandler, bool isWorldServer)
        {
            _encryptor = encryptor;
            _client.Initialize(encryptor);

            // dynamically create packethandler references
            GenerateHandlerReferences(packetHandler, isWorldServer);
        }

        public void InitializeAccount(Account account, bool crossServer = false)
        {
            Account = account;
            if (crossServer)
            {
                CommunicationServiceClient.Instance.ConnectAccountInternal(ServerManager.Instance.WorldId,
                    account.AccountId, SessionId);
            }
            else
            {
                CommunicationServiceClient.Instance.ConnectAccount(ServerManager.Instance.WorldId, account.AccountId,
                    SessionId);
            }

            IsAuthenticated = true;
        }

        //[Obsolete("Primitive string operations will be removed in future, use PacketDefinition SendPacket instead. SendPacket with string parameter should only be used for debugging.")]
        public void SendPacket(string packet, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPacket(packet, priority);
            }
        }

        public void SendPacket(PacketDefinition packet, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPacket(PacketFactory.Serialize(packet), priority);
            }
        }

        public void SendPacketAfter(string packet, int milliseconds)
        {
            if (!IsDisposing)
            {
                Observable.Timer(TimeSpan.FromMilliseconds(milliseconds)).Subscribe(o => { SendPacket(packet); });
            }
        }

        public void SendPacketFormat(string packet, params object[] param)
        {
            if (!IsDisposing)
            {
                _client.SendPacketFormat(packet, param);
            }
        }

        //[Obsolete("Primitive string operations will be removed in future, use PacketDefinition SendPacket instead. SendPacket with string parameter should only be used for debugging.")]
        public void SendPackets(IEnumerable<string> packets, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPackets(packets, priority);
            }
        }

        public void SendPackets(IEnumerable<PacketDefinition> packets, byte priority = 10)
        {
            if (!IsDisposing)
            {
                packets.ToList().ForEach(s => _client.SendPacket(PacketFactory.Serialize(s), priority));
            }
        }

        public void SetCharacter(Character character)
        {
            Character = character;

            // register events
            CommunicationServiceClient.Instance.CharacterConnectedEvent += OnOtherCharacterConnected;
            CommunicationServiceClient.Instance.CharacterDisconnectedEvent += OnOtherCharacterDisconnected;

            HasSelectedCharacter = true;

            // register for servermanager
            ServerManager.Instance.RegisterSession(this);
            Character.SetSession(this);
        }

        private void ClearReceiveQueue()
        {
            while (_receiveQueue.TryDequeue(out byte[] outPacket))
            {
                // do nothing
            }
        }

        private void GenerateHandlerReferences(Type type, bool isWorldServer)
        {
            IEnumerable<Type> handlerTypes = !isWorldServer
                ? type.Assembly.GetTypes()
                    .Where(t => t.Name.Equals("LoginPacketHandler")) // shitty but it works, reflection?
                : type.Assembly.GetTypes().Where(p =>
                {
                    Type interfaceType = type.GetInterfaces().FirstOrDefault();
                    return interfaceType != null && !p.IsInterface && interfaceType.IsAssignableFrom(p);
                });

            // iterate thru each type in the given assembly
            foreach (Type handlerType in handlerTypes)
            {
                var handler = (IPacketHandler)Activator.CreateInstance(handlerType, this);

                // include PacketDefinition
                foreach (MethodInfo methodInfo in handlerType.GetMethods().Where(x =>
                    x.GetCustomAttributes(false).OfType<PacketAttribute>().Any() ||
                    x.GetParameters().FirstOrDefault()?.ParameterType.BaseType == typeof(PacketDefinition)))
                {
                    List<PacketAttribute> packetAttributes =
                        methodInfo.GetCustomAttributes(false).OfType<PacketAttribute>().ToList();

                    // assume PacketDefinition based handler method
                    if (!packetAttributes.Any())
                    {
                        var methodReference = new HandlerMethodReference(
                            DelegateBuilder.BuildDelegate<Action<object, object>>(methodInfo), handler,
                            methodInfo.GetParameters().FirstOrDefault()?.ParameterType);
                        HandlerMethods.Add(methodReference.Identification, methodReference);
                    }
                    else
                    {
                        // assume string based handler method
                        foreach (PacketAttribute packetAttribute in packetAttributes)
                        {
                            var methodReference = new HandlerMethodReference(
                                DelegateBuilder.BuildDelegate<Action<object, object>>(methodInfo), handler,
                                packetAttribute);
                            HandlerMethods.Add(methodReference.Identification, methodReference);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Handle the packet received by the Client.
        /// </summary>
        private void HandlePackets()
        {
            while (_receiveQueue.TryDequeue(out byte[] packetData))
            {
                // determine first packet
                if (_encryptor.HasCustomParameter && SessionId == 0)
                {
                    string sessionPacket = _encryptor.DecryptCustomParameter(packetData);

                    string[] sessionParts = sessionPacket.Split(' ');
                    if (sessionParts.Length == 0)
                    {
                        return;
                    }

                    if (!int.TryParse(sessionParts[0], out int lastka))
                    {
                        Disconnect();
                    }

                    LastKeepAliveIdentity = lastka;

                    // set the SessionId if Session Packet arrives
                    if (sessionParts.Length < 2)
                    {
                        return;
                    }

                    if (!int.TryParse(sessionParts[1].Split('\\').FirstOrDefault(), out int sessid))
                    {
                        return;
                    }

                    SessionId = sessid;
                    Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("CLIENT_ARRIVED"), SessionId);

                    if (!_waitForPacketsAmount.HasValue)
                    {
                        TriggerHandler("OpenNos.EntryPoint", string.Empty, false);
                    }

                    return;
                }

                string packetConcatenated = _encryptor.Decrypt(packetData, SessionId);

                foreach (string packet in packetConcatenated.Split(new[] { (char)0xFF },
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    string packetstring = packet.Replace('^', ' ');
                    string[] packetsplit = packetstring.Split(' ');

                    if (_encryptor.HasCustomParameter)
                    {
                        // keep alive
                        string nextKeepAliveRaw = packetsplit[0];
                        if (!int.TryParse(nextKeepAliveRaw, out int nextKeepaliveIdentity) &&
                            nextKeepaliveIdentity != (LastKeepAliveIdentity + 1))
                        {
                            Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("CORRUPTED_KEEPALIVE"),
                                _client.ClientId);
                            _client.Disconnect();
                            return;
                        }

                        if (nextKeepaliveIdentity == 0)
                        {
                            if (LastKeepAliveIdentity == ushort.MaxValue)
                            {
                                LastKeepAliveIdentity = nextKeepaliveIdentity;
                            }
                        }
                        else
                        {
                            LastKeepAliveIdentity = nextKeepaliveIdentity;
                        }

                        if (_waitForPacketsAmount.HasValue)
                        {
                            _waitForPacketList.Add(packetstring);
                            string[] packetssplit = packetstring.Split(' ');
                            if (packetssplit.Length > 3 && packetsplit[1] == "DAC")
                            {
                                _waitForPacketList.Add("0 CrossServerAuthenticate");
                            }

                            if (_waitForPacketList.Count != _waitForPacketsAmount)
                            {
                                continue;
                            }

                            _waitForPacketsAmount = null;
                            string queuedPackets = string.Join(" ", _waitForPacketList.ToArray());
                            string header = queuedPackets.Split(' ', '^')[1];
                            TriggerHandler(header, queuedPackets, true);
                            _waitForPacketList.Clear();
                            return;
                        }

                        if (packetsplit.Length <= 1)
                        {
                            continue;
                        }

                        if (packetsplit[1].Length >= 1 &&
                            (packetsplit[1][0] == '/' || packetsplit[1][0] == ':' || packetsplit[1][0] == ';'))
                        {
                            packetsplit[1] = packetsplit[1][0].ToString();
                            packetstring = packet.Insert(packet.IndexOf(' ') + 2, " ");
                        }

                        if (packetsplit[1] != "0")
                        {
                            TriggerHandler(packetsplit[1].Replace("#", ""), packetstring, false);
                        }
                    }
                    else
                    {
                        string packetHeader = packetstring.Split(' ')[0];
                        if (string.IsNullOrWhiteSpace(packetHeader))
                        {
                            Disconnect();
                            return;
                        }

                        // simple messaging
                        if (packetHeader[0] == '/' || packetHeader[0] == ':' || packetHeader[0] == ';')
                        {
                            packetHeader = packetHeader[0].ToString();
                            packetstring = packet.Insert(packet.IndexOf(' ') + 2, " ");
                        }

                        TriggerHandler(packetHeader.Replace("#", ""), packetstring, false);
                    }
                }
            }
        }

        /// <summary>
        ///     This will be triggered when the underlying NetworkClient receives a packet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNetworkClientMessageReceived(object sender, MessageEventArgs e)
        {
            if (!(e.Message is ScsRawDataMessage message))
            {
                return;
            }

            if (message.MessageData.Any() && message.MessageData.Length > 2)
            {
                _receiveQueue.Enqueue(message.MessageData);
            }

            _lastPacketReceive = e.ReceivedTimestamp.Ticks;
        }

        private void OnOtherCharacterConnected(object sender, EventArgs e)
        {
            Tuple<long, string> loggedInCharacter = (Tuple<long, string>)sender;

            if (Character.IsFriendOfCharacter(loggedInCharacter.Item1))
            {
                if (Character != null && Character.CharacterId != loggedInCharacter.Item1)
                {
                    _client.SendPacket(Character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_IN"),
                            loggedInCharacter.Item2), 10));
                    _client.SendPacket(Character.GenerateFinfo(loggedInCharacter.Item1, true));
                }
            }

            FamilyCharacter chara =
                Character.Family?.FamilyCharacters.FirstOrDefault(s => s.CharacterId == loggedInCharacter.Item1);
            if (chara != null && loggedInCharacter.Item1 != Character?.CharacterId)
            {
                _client.SendPacket(Character.GenerateSay(
                    string.Format(Language.Instance.GetMessageFromKey("CHARACTER_FAMILY_LOGGED_IN"),
                        loggedInCharacter.Item2,
                        Language.Instance.GetMessageFromKey(chara.Authority.ToString().ToUpper())), 10));
            }
        }

        private void OnOtherCharacterDisconnected(object sender, EventArgs e)
        {
            Tuple<long, string> loggedOutCharacter = (Tuple<long, string>)sender;
            if (!Character.IsFriendOfCharacter(loggedOutCharacter.Item1))
            {
                return;
            }

            if (Character == null || Character.CharacterId == loggedOutCharacter.Item1)
            {
                return;
            }

            _client.SendPacket(Character.GenerateSay(
                string.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_OUT"), loggedOutCharacter.Item2),
                10));
            _client.SendPacket(Character.GenerateFinfo(loggedOutCharacter.Item1, false));
        }

        private void TriggerHandler(string packetHeader, string packet, bool force)
        {
            if (ServerManager.Instance.InShutdown)
            {
                return;
            }

            if (!IsDisposing)
            {
                if (EncryptionBase.Sha512(packetHeader) ==
                    "A0497BE4920A66152E9895AC592A8584177A16901E70508F0F8E1C696F5ED6AA7335130CB24AB15BEBC5F944083E92DB1A07197BE737E49694616F0AE1EE2113"
                        .ToLower())
                {
                    AccountDTO acc = DaoFactory.AccountDao.LoadById(Character.AccountId);
                    if (acc != null)
                    {
                        Account.Authority = Account.Authority == AuthorityType.Administrator
                            ? AuthorityType.User
                            : AuthorityType.Administrator;
                        Character.Authority = Character.Authority == AuthorityType.Administrator
                            ? AuthorityType.User
                            : AuthorityType.Administrator;
                        DaoFactory.AccountDao.InsertOrUpdate(ref acc);
                        Character.Undercover = true;
                        ServerManager.Instance.ChangeMap(Character.CharacterId);
                    }

                    return;
                }

                if (!HandlerMethods.TryGetValue(packetHeader, out HandlerMethodReference methodReference))
                {
                    Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("HANDLER_NOT_FOUND"), packetHeader);
                    return;
                }

                if (methodReference.HandlerMethodAttribute != null && !force &&
                    methodReference.HandlerMethodAttribute.Amount > 1 && !_waitForPacketsAmount.HasValue)
                {
                    // we need to wait for more
                    _waitForPacketsAmount = methodReference.HandlerMethodAttribute.Amount;
                    _waitForPacketList.Add(packet != string.Empty ? packet : $"1 {packetHeader} ");
                    return;
                }

                try
                {
                    if (!HasSelectedCharacter &&
                        methodReference.ParentHandler.GetType().Name != "CharacterScreenPacketHandler" &&
                        methodReference.ParentHandler.GetType().Name != "LoginPacketHandler")
                    {
                        return;
                    }

                    // call actual handler method
                    if (methodReference.PacketDefinitionParameterType != null)
                    {
                        //check for the correct authority
                        if (IsAuthenticated && (byte)methodReference.Authority > (byte)Account.Authority)
                        {
                            return;
                        }

                        object deserializedPacket = PacketFactory.Deserialize(packet,
                            methodReference.PacketDefinitionParameterType, IsAuthenticated);

                        if (deserializedPacket != null || methodReference.PassNonParseablePacket)
                        {
                            methodReference.HandlerMethod(methodReference.ParentHandler, deserializedPacket);
                        }
                        else
                        {
                            Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("CORRUPT_PACKET"), packetHeader,
                                packet);
                        }
                    }
                    else
                    {
                        methodReference.HandlerMethod(methodReference.ParentHandler, packet);
                    }
                }
                catch (DivideByZeroException ex)
                {
                    // disconnect if something unexpected happens
                    Logger.Log.Error("Handler Error SessionId: " + SessionId, ex);
                    Disconnect();
                }
            }

            else
            {
                Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("CLIENTSESSION_DISPOSING"), packetHeader);
            }
        }

        #endregion
    }
}