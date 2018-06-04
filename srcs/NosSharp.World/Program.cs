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
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using log4net;
using ON.NW.Customisation.Helpers;
using ON.NW.Customisation.NewCharCustomisation;
using ON.NW.World.Resource;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Core.Utilities;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.DAL.EF.Entities;
using OpenNos.DAL.EF.Helpers;
using OpenNos.GameObject;
using OpenNos.GameObject.Extensions;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Packets.ClientPackets;
using OpenNos.Handler;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using Account = OpenNos.DAL.EF.Entities.Account;
using BCard = OpenNos.GameObject.Buff.BCard;
using BoxInstance = OpenNos.GameObject.Item.Instance.BoxInstance;
using Card = OpenNos.GameObject.Buff.Card;
using Character = OpenNos.DAL.EF.Entities.Character;
using CharacterQuest = OpenNos.GameObject.CharacterQuest;
using CharacterSkill = OpenNos.GameObject.CharacterSkill;
using Family = OpenNos.GameObject.Family;
using FamilyCharacter = OpenNos.GameObject.FamilyCharacter;
using ItemInstance = OpenNos.GameObject.Item.Instance.ItemInstance;
using MapMonster = OpenNos.GameObject.Map.MapMonster;
using MapNpc = OpenNos.GameObject.Map.MapNpc;
using Mate = OpenNos.GameObject.Mate;
using NpcMonster = OpenNos.GameObject.Npc.NpcMonster;
using NpcMonsterSkill = OpenNos.GameObject.NpcMonsterSkill;
using Portal = OpenNos.GameObject.Portal;
using Quest = OpenNos.GameObject.Quest;
using Recipe = OpenNos.GameObject.Recipe;
using ScriptedInstance = OpenNos.GameObject.ScriptedInstance;
using Shop = OpenNos.GameObject.Shop;
using Skill = OpenNos.GameObject.Skill;
using SpecialistInstance = OpenNos.GameObject.Item.Instance.SpecialistInstance;
using WearableInstance = OpenNos.GameObject.Item.Instance.WearableInstance;

namespace ON.NW.World
{
    public class Program
    {
        #region Delegates

        public delegate bool EventHandler(CtrlType sig);

        #endregion

        #region Enums

        public enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        #endregion

        #region Members

        private static EventHandler _exitHandler;
        private static ManualResetEvent _run = new ManualResetEvent(true);

        #endregion

        #region Methods

        private static short _port;

        private static void Welcome()
        {
            Console.Title = string.Format(LocalizedResources.WORLD_SERVER_CONSOLE_TITLE, 0, 0, 0, 0);
            _port = Convert.ToInt16(ConfigurationManager.AppSettings["WorldPort"]);
            const string text = "N# - World Server";
            int offset = Console.WindowWidth / 2 + text.Length / 2;
            string separator = new string('=', Console.WindowWidth);
            Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);
        }

        private static void CustomisationRegistration()
        {
            const string configPath = "./config/";
            var baseCharacter = ConfigurationHelper.Load<BaseCharacter>(configPath + nameof(BaseCharacter) + ".json", true);
            Logger.Log.Info("[CUSTOMIZER] BaseCharacter Loaded !");
            DependencyContainer.Instance.Register(baseCharacter);
            var baseQuicklist = ConfigurationHelper.Load<BaseQuicklist>(configPath + nameof(BaseQuicklist) + ".json", true);
            Logger.Log.Info("[CUSTOMIZER] BaseQuicklist Loaded !");
            DependencyContainer.Instance.Register(baseQuicklist);
            var baseInventory = ConfigurationHelper.Load<BaseInventory>(configPath + nameof(BaseInventory) + ".json", true);
            Logger.Log.Info("[CUSTOMIZER] BaseInventory Loaded !");
            DependencyContainer.Instance.Register(baseInventory);
            var baseSkill = ConfigurationHelper.Load<BaseSkill>(configPath + nameof(BaseSkill) + ".json", true);
            Logger.Log.Info("[CUSTOMIZER] BaseSkill Loaded !");
            DependencyContainer.Instance.Register(baseSkill);
        }

        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            // initialize Loggers
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
            Welcome();
            CustomisationRegistration();

            // initialize api
            if (CommunicationServiceClient.Instance.Authenticate(ConfigurationManager.AppSettings["MasterAuthKey"]))
            {
                Logger.Log.Info(Language.Instance.GetMessageFromKey("API_INITIALIZED"));
            }

            // initialize DB
            if (DataAccessHelper.Initialize())
            {
                // register mappings for DAOs, Entity -> GameObject and GameObject -> Entity
                RegisterMappings();

                // initialilize maps
                ServerManager.Instance.Initialize();
            }
            else
            {
                Console.ReadKey();
                return;
            }

            // TODO: initialize ClientLinkManager initialize PacketSerialization
            PacketFactory.Initialize<WalkPacket>();
            string ip = ConfigurationManager.AppSettings["IPADDRESS"];
            if (!bool.TryParse(ConfigurationManager.AppSettings["AutoReboot"], out bool autoreboot))
            {
                autoreboot = false;
            }

            try
            {
                _exitHandler += ExitHandler;
                if (autoreboot)
                {
                    AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
                    AppDomain.CurrentDomain.ProcessExit += ProcessExitHandler;
                }

                NativeMethods.SetConsoleCtrlHandler(_exitHandler, true);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }

            portloop:
            try
            {
                NetworkManager<WorldEncryption> unused =
                    new NetworkManager<WorldEncryption>(ConfigurationManager.AppSettings["IPADDRESS"], _port, typeof(CommandPacketHandler), typeof(LoginEncryption), true);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10048)
                {
                    _port++;
                    Logger.Log.Info("Port already in use! Incrementing...");
                    goto portloop;
                }

                Logger.Log.Error("General Error", ex);
                Environment.Exit(1);
            }

            ServerManager.Instance.ServerGroup = ConfigurationManager.AppSettings["ServerGroup"];
            int sessionLimit = Convert.ToInt32(ConfigurationManager.AppSettings["SessionLimit"]);
            int? newChannelId = CommunicationServiceClient.Instance.RegisterWorldServer(new SerializableWorldServer(ServerManager.Instance.WorldId, ip, _port, sessionLimit,
                ServerManager.Instance.ServerGroup));

            if (newChannelId.HasValue)
            {
                ServerManager.Instance.ChannelId = newChannelId.Value;
                ServerManager.Instance.IpAddress = ip;
                ServerManager.Instance.Port = _port;
                ServerManager.Instance.AccountLimit = sessionLimit;
                Console.Title = string.Format(Language.Instance.GetMessageFromKey("WORLD_SERVER_CONSOLE_TITLE"), ServerManager.Instance.ChannelId, ServerManager.Instance.Sessions.Count(),
                    ServerManager.Instance.IpAddress, ServerManager.Instance.Port);
            }
            else
            {
                Logger.Log.ErrorFormat("Could not retrieve ChannelId from Web API.");
                Console.ReadKey();
            }
        }

        private static bool ExitHandler(CtrlType sig)
        {
            ServerManager.Instance.InShutdown = true;
            ServerManager.Instance.SaveAll();

            ServerManager.Instance.Shout(string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 5));
            Thread.Sleep(10000);
            CommunicationServiceClient.Instance.UnregisterWorldServer(ServerManager.Instance.WorldId);
            return false;
        }

        private static void ProcessExitHandler(object sender, EventArgs eventArgs)
        {
            ServerManager.Instance.Shout(string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 5));
            ServerManager.Instance.InShutdown = true;
            ServerManager.Instance.DisconnectAll();

            Thread.Sleep(10000);
            Process.GetCurrentProcess().CloseMainWindow();
            Environment.Exit(84);
        }

        private static void UnhandledExceptionHandler(object sender, EventArgs eventArgs)
        {
            ServerManager.Instance.Shout(string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 5));
            ServerManager.Instance.InShutdown = true;
            ServerManager.Instance.DisconnectAll();

            Thread.Sleep(10000);
            CommunicationServiceClient.Instance.UnregisterWorldServer(ServerManager.Instance.WorldId);
            Process.Start("ON.NW.World.exe");
        }

        private static void RegisterMappings()
        {
            DaoExtensions.RegisterMapping<Account>(typeof(OpenNos.GameObject.Account));
            DaoExtensions.RegisterMapping<Character>(typeof(OpenNos.GameObject.Character));
            DaoExtensions.RegisterMapping<EquipmentOption>(typeof(EquipmentOptionDTO));


            // register mappings for items
            DaoFactory.IteminstanceDao.RegisterMapping(typeof(BoxInstance));
            DaoFactory.IteminstanceDao.RegisterMapping(typeof(SpecialistInstance));
            DaoFactory.IteminstanceDao.RegisterMapping(typeof(WearableInstance));
            DaoFactory.IteminstanceDao.InitializeMapper(typeof(ItemInstance));

            // entities
            DaoFactory.AccountDao.RegisterMapping(typeof(OpenNos.GameObject.Account)).InitializeMapper();
            DaoFactory.EquipmentOptionDao.RegisterMapping(typeof(EquipmentOptionDTO)).InitializeMapper();
            DaoFactory.CharacterDao.RegisterMapping(typeof(OpenNos.GameObject.Character)).InitializeMapper();
            DaoFactory.CharacterRelationDao.RegisterMapping(typeof(CharacterRelationDTO)).InitializeMapper();
            DaoFactory.CharacterSkillDao.RegisterMapping(typeof(CharacterSkill)).InitializeMapper();
            DaoFactory.CharacterQuestDao.RegisterMapping(typeof(CharacterQuestDTO)).InitializeMapper();
            DaoFactory.CharacterQuestDao.RegisterMapping(typeof(CharacterQuest)).InitializeMapper();
            DaoFactory.ComboDao.RegisterMapping(typeof(ComboDTO)).InitializeMapper();
            DaoFactory.DropDao.RegisterMapping(typeof(DropDTO)).InitializeMapper();
            DaoFactory.GeneralLogDao.RegisterMapping(typeof(GeneralLogDTO)).InitializeMapper();
            DaoFactory.ItemDao.RegisterMapping(typeof(ItemDTO)).InitializeMapper();
            DaoFactory.BazaarItemDao.RegisterMapping(typeof(BazaarItemDTO)).InitializeMapper();
            DaoFactory.MailDao.RegisterMapping(typeof(MailDTO)).InitializeMapper();
            DaoFactory.RollGeneratedItemDao.RegisterMapping(typeof(RollGeneratedItemDTO)).InitializeMapper();
            DaoFactory.MapDao.RegisterMapping(typeof(MapDTO)).InitializeMapper();
            DaoFactory.MapMonsterDao.RegisterMapping(typeof(MapMonster)).InitializeMapper();
            DaoFactory.MapNpcDao.RegisterMapping(typeof(MapNpc)).InitializeMapper();
            DaoFactory.FamilyDao.RegisterMapping(typeof(FamilyDTO)).InitializeMapper();
            DaoFactory.FamilyCharacterDao.RegisterMapping(typeof(FamilyCharacterDTO)).InitializeMapper();
            DaoFactory.FamilyLogDao.RegisterMapping(typeof(FamilyLogDTO)).InitializeMapper();
            DaoFactory.MapTypeDao.RegisterMapping(typeof(MapTypeDTO)).InitializeMapper();
            DaoFactory.MapTypeMapDao.RegisterMapping(typeof(MapTypeMapDTO)).InitializeMapper();
            DaoFactory.NpcMonsterDao.RegisterMapping(typeof(NpcMonster)).InitializeMapper();
            DaoFactory.NpcMonsterSkillDao.RegisterMapping(typeof(NpcMonsterSkill)).InitializeMapper();
            DaoFactory.PenaltyLogDao.RegisterMapping(typeof(PenaltyLogDTO)).InitializeMapper();
            DaoFactory.PortalDao.RegisterMapping(typeof(PortalDTO)).InitializeMapper();
            DaoFactory.PortalDao.RegisterMapping(typeof(Portal)).InitializeMapper();
            DaoFactory.QuestDao.RegisterMapping(typeof(QuestDTO)).InitializeMapper();
            DaoFactory.QuestDao.RegisterMapping(typeof(Quest)).InitializeMapper();
            DaoFactory.QuestLogDao.RegisterMapping(typeof(QuestLogDTO)).InitializeMapper();
            DaoFactory.QuestRewardDao.RegisterMapping(typeof(QuestRewardDTO)).InitializeMapper();
            DaoFactory.QuestObjectiveDao.RegisterMapping(typeof(QuestObjectiveDTO)).InitializeMapper();
            DaoFactory.QuicklistEntryDao.RegisterMapping(typeof(QuicklistEntryDTO)).InitializeMapper();
            DaoFactory.RecipeDao.RegisterMapping(typeof(Recipe)).InitializeMapper();
            DaoFactory.RecipeItemDao.RegisterMapping(typeof(RecipeItemDTO)).InitializeMapper();
            DaoFactory.MinilandObjectDao.RegisterMapping(typeof(MinilandObjectDTO)).InitializeMapper();
            DaoFactory.MinilandObjectDao.RegisterMapping(typeof(MapDesignObject)).InitializeMapper();
            DaoFactory.RaidLogDao.RegisterMapping(typeof(RaidLogDTO)).InitializeMapper();
            DaoFactory.RespawnDao.RegisterMapping(typeof(RespawnDTO)).InitializeMapper();
            DaoFactory.RespawnMapTypeDao.RegisterMapping(typeof(RespawnMapTypeDTO)).InitializeMapper();
            DaoFactory.ShopDao.RegisterMapping(typeof(Shop)).InitializeMapper();
            DaoFactory.ShopItemDao.RegisterMapping(typeof(ShopItemDTO)).InitializeMapper();
            DaoFactory.ShopSkillDao.RegisterMapping(typeof(ShopSkillDTO)).InitializeMapper();
            DaoFactory.CardDao.RegisterMapping(typeof(CardDTO)).InitializeMapper();
            DaoFactory.BCardDao.RegisterMapping(typeof(BCardDTO)).InitializeMapper();
            DaoFactory.CardDao.RegisterMapping(typeof(Card)).InitializeMapper();
            DaoFactory.BCardDao.RegisterMapping(typeof(BCard)).InitializeMapper();
            DaoFactory.SkillDao.RegisterMapping(typeof(Skill)).InitializeMapper();
            DaoFactory.MateDao.RegisterMapping(typeof(MateDTO)).InitializeMapper();
            DaoFactory.MateDao.RegisterMapping(typeof(Mate)).InitializeMapper();
            DaoFactory.TeleporterDao.RegisterMapping(typeof(TeleporterDTO)).InitializeMapper();
            DaoFactory.StaticBonusDao.RegisterMapping(typeof(StaticBonusDTO)).InitializeMapper();
            DaoFactory.StaticBuffDao.RegisterMapping(typeof(StaticBuffDTO)).InitializeMapper();
            DaoFactory.FamilyDao.RegisterMapping(typeof(Family)).InitializeMapper();
            DaoFactory.FamilyCharacterDao.RegisterMapping(typeof(FamilyCharacter)).InitializeMapper();
            DaoFactory.ScriptedInstanceDao.RegisterMapping(typeof(ScriptedInstanceDTO)).InitializeMapper();
            DaoFactory.ScriptedInstanceDao.RegisterMapping(typeof(ScriptedInstance)).InitializeMapper();
            DaoFactory.LogChatDao.RegisterMapping(typeof(LogChatDTO)).InitializeMapper();
            DaoFactory.LogCommandsDao.RegisterMapping(typeof(LogCommandsDTO)).InitializeMapper();
            DaoFactory.UpgradeLogDao.RegisterMapping(typeof(UpgradeLogDTO)).InitializeMapper();
            DaoFactory.ExchangeLogDao.RegisterMapping(typeof(ExchangeLogDTO)).InitializeMapper();
        }

        public class NativeMethods
        {
            [DllImport("Kernel32")]
            internal static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
        }

        #endregion
    }
}