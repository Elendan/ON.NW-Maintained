using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Handling;
using OpenNos.Data;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Packets.HomePackets;
using System.Linq;

namespace OpenNos.Handler
{
    public class HomeSystemPacketHandler : IPacketHandler
    {
		private ClientSession Session { get; }
		public HomeSystemPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        ///     This method will handle the
        /// </summary>
        public void SetHome(SetHomePacket packet)
        {
			if (packet != null)
			{
				if (Session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
				{
					Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
					return;
				}

				if (packet.HomeId < 1 || packet.HomeId > 5)
				{
					return;
				}

				RespawnDTO resp = Session.Character.Respawns.Find(s => s.RespawnMapTypeId == packet.HomeId + 50);
				if (resp == null)
				{
					resp = new RespawnDTO
					{
						CharacterId = Session.Character.CharacterId,
						MapId = Session.Character.MapId,
						X = Session.Character.MapX,
						Y = Session.Character.MapY,
						RespawnMapTypeId = (long)packet.HomeId + 50
					};
					Session.Character.Respawns.Add(resp);
					Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("HOMEPOINT_SET"), 10));
				}
				else
				{
					Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("HOMEPOINT_MODIFIED"), 10));
					resp.X = Session.Character.MapX;
					resp.Y = Session.Character.MapY;
					resp.MapId = Session.Character.MapInstance.Map.MapId;
				}


				if (ServerManager.Instance.ChannelId == 51 || ServerManager.Instance.GetMapInstance(
					ServerManager.Instance.GetBaseMapInstanceIdByMapId(resp.MapId)).Map.MapTypes.Any(
					s => s.MapTypeId == (short)MapTypeEnum.Act4 || s.MapTypeId == (short)MapTypeEnum.Act42))
				{
					Session.SendPacket(
						Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
					return;
				}
			}
			else
			{
				Session.SendPacket(Session.Character.GenerateSay(packet.ToString(), 10));
				return;
			}

			// if home already exist replace it
		}

        /// <summary>
        ///     This method will handle the unsethome packet
        /// </summary>
        public void UnsetHome(UnsetHomePacket packet)
        {
            if (packet == null)
            {
            }

            // remove home
        }


        /// <summary>
        /// </summary>
        public void Home(HomePacket packet)
        {
            if (packet == null)
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

            // X = delay to tp (FileConfiguration)
            // Set WaitingForTeleportation flag to true
            // new Task teleport in X milliseconds
        }
    }
}