using System;
using System.Collections.Generic;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Handling;
using OpenNos.Data;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Packets.HomePackets;
using System.Linq;
using System.Reactive.Linq;
using OpenNos.DAL;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler
{
    public class HomeSystemPacketHandler : IPacketHandler
    {
		public ClientSession Session { get; }
		public HomeSystemPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        ///     This method will handle the
        /// </summary>
        public void SetHome(SetHomePacket packet)
        {
            if (packet?.Name == null)
            {
                return;
            }

            if (Session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                return;
            }

            if (Session.Character.Homes.Count() >= 5 && Session.Account.Authority < AuthorityType.GameMaster)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("TOO_MANY_HOMEPOINTS")));
                return;
            }

            CharacterHomeDTO home = Session.Character.Homes.FirstOrDefault(s => s.Name == packet.Name);
            if (home == null)
            {
                home = new CharacterHomeDTO
                {
                    CharacterId = Session.Character.CharacterId,
                    Id = Guid.NewGuid(),
                    MapId = Session.Character.MapId,
                    MapX = Session.Character.PositionX,
                    MapY = Session.Character.PositionY,
                    Name = packet.Name
                };
                DaoFactory.CharacterHomeDao.InsertOrUpdate(ref home);
                ServerManager.Instance.RefreshHomes();
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("HOMEPOINT_SET"), 10));
            }
            else
            {
                home.MapX = Session.Character.MapX;
                home.MapY = Session.Character.MapY;
                home.MapId = Session.Character.MapInstance.Map.MapId;
                DaoFactory.CharacterHomeDao.InsertOrUpdate(ref home);
                ServerManager.Instance.RefreshHomes();
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("HOMEPOINT_MODIFIED"), 10));
            }
        }

        /// <summary>
        ///     This method will handle the unsethome packet
        /// </summary>
        public void UnsetHome(UnsetHomePacket packet)
        {
            if (packet?.Name == null)
            {
                return;
            }

            CharacterHomeDTO home = Session.Character.Homes.FirstOrDefault(s => s.Name == packet.Name);

            if (home == null)
            {
                return;
            }

            DaoFactory.CharacterHomeDao.DeleteByName(Session.Character.CharacterId, packet.Name);
            ServerManager.Instance.RefreshHomes();
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("HOMEPOINT_DELETED"), 10));
        }


        /// <summary>
        /// </summary>
        public void Home(HomePacket packet)
        {
            if (packet?.Name == null)
            {
                return;
            }

            if (Session.Character.HasShopOpened)
            {
				Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CLOSE_SHOP"), 11));
                return;
            }

            if (Session.Character.InExchangeOrTrade)
            {
				Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CLOSE_EXCHANGE"), 11));
            }

            CharacterHomeDTO home = Session.Character.Homes.FirstOrDefault(s => s.Name == packet.Name);

            if (home == null)
            {
                return;
            }

            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("TELEPORTING_IN_SECONDS")));
            Session.Character.MapInstance?.Broadcast(Session.Character.GenerateEff(3999));
            Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(s => 
            {
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, home.MapId, home.MapX, home.MapY);
                Session.Character.MapInstance?.Broadcast(Session.Character.GenerateEff(4000));
            });
        }

        public void ListHome(ListHomePacket packet)
        {
            if (packet == null)
            {
                return;
            }

            if (Session.Character.Homes == null || !Session.Character.Homes.Any())
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_HOMES"), 10));
                return;
            }

            foreach (CharacterHomeDTO home in Session.Character.Homes)
            {
                Session.SendPacket(Session.Character.GenerateSay($"{home.Name}: MapId: {home.MapId}, MapX: {home.MapX}, MapY: {home.MapY}", 10));
            }
        }
    }
}