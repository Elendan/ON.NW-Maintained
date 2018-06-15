﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.GAMES
{
    class SheepGame
    {
        #region Methods

        public static void GenerateSheepGames()
        {
            Thread.Sleep(5 * 1000);
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHEEP_STARTED"), 1));
            ServerManager.Instance.Broadcast("qnaml 100 #guri^506 The Sheep Game just started! Join now !");
            ServerManager.Instance.EventInWaiting = true;
            Thread.Sleep(30 * 1000);
            ServerManager.Instance.Sessions.Where(s => s.Character?.IsWaitingForEvent == false).ToList().ForEach(s => s.SendPacket("esf 1"));
            ServerManager.Instance.EventInWaiting = false;

            IEnumerable<ClientSession> sessions = ServerManager.Instance.Sessions.Where(s => s.Character?.IsWaitingForEvent == true && s.Character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance);
            List<Tuple<MapInstance, byte>> maps = new List<Tuple<MapInstance, byte>>();
            MapInstance map = ServerManager.Instance.GenerateMapInstance(2009, MapInstanceType.EventGameInstance, new InstanceBag());
            maps.Add(new Tuple<MapInstance, byte>(map, 1));
            if (map != null)
            {
                foreach (ClientSession sess in sessions)
                {
                    ServerManager.Instance.TeleportOnRandomPlaceInMap(sess, map.MapInstanceId);
                    sess.SendPacket("bsinfo 2 4 0 0");
                }
                ServerManager.Instance.Sessions.Where(s => s.Character != null).ToList().ForEach(s => s.Character.IsWaitingForEvent = false);
                ServerManager.Instance.StartedEvents.Remove(EventType.SHEEPGAME);

            }

            if (map.Sessions.Count() < 1)
            {
                map.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_NOT_ENOUGH_PLAYERS"), 0));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(5), new EventContainer(map, EventActionType.DISPOSEMAP, null));
                return;
            }
            SheepThread task = new SheepThread();
            Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(X => task.Run(map));
        }



        #endregion

        #region Classes

        public class SheepThread
        {

            public bool Spawn { get; set; }

            #region Members

            private MapInstance _map;

            #endregion

            #region Methods

            public void Run(MapInstance map)
            {
                _map = map;

                foreach (ClientSession session in _map.Sessions)
                {
                    ServerManager.Instance.TeleportOnRandomPlaceInMap(session, map.MapInstanceId);
                    if (session.Character.IsVehicled)
                    {
                        session.Character.RemoveVehicle();
                    }
                    if (session.Character.UseSp)
                    {
                        session.Character.LastSp = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                        ItemInstance specialist = session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                        if (specialist != null)
                        {
                            removeSP(session, specialist.ItemVNum);
                        }
                    }
                    Spawn = true;
                    //session.Character.CanAttack = true;
                    session.Character.Speed = 5;
                    session.Character.IsVehicled = true;
                    session.Character.IsCustomSpeed = true;
                    session.Character.Morph = 1009;
                    session.Character.ArenaWinner = 0;
                    session.Character.MorphUpgrade = 0;
                    session.Character.MorphUpgrade2 = 0;
                    session.SendPacket(session.Character.GenerateCond());
                    session.Character.LastSpeedChange = DateTime.Now;
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                    session.SendPacket($"eff_s 1 {session.Character.CharacterId} 4323");
                    _map.Broadcast("srlst 0");
                    session.SendPacket("say 1 -1 10 Dans 30 secondes Le raid de la Ferme des moutons commence bientôt.");
                    Observable.Timer(TimeSpan.FromSeconds(30)).Subscribe(o =>
                    {
                        session.SendPacket("say 1 -1 10 Dans 10 secondes Le raid de la Ferme des moutons commence bientôt.");
                    });
                    Observable.Timer(TimeSpan.FromSeconds(35)).Subscribe(o =>
                    {
                        session.SendPacket("say 1 -1 10 Dans 5 secondes Le raid de la Ferme des moutons commence bientôt.");
                    });
                    Observable.Timer(TimeSpan.FromSeconds(36)).Subscribe(o =>
                    {
                        session.SendPacket("say 1 -1 10 Dans 4 secondes Le raid de la Ferme des moutons commence bientôt.");
                    });
                    Observable.Timer(TimeSpan.FromSeconds(37)).Subscribe(o =>
                    {
                        session.SendPacket("say 1 -1 10 Dans 3 secondes Le raid de la Ferme des moutons commence bientôt.");
                    });
                    Observable.Timer(TimeSpan.FromSeconds(38)).Subscribe(o =>
                    {
                        session.SendPacket("say 1 -1 10 Dans 2 secondes Le raid de la Ferme des moutons commence bientôt.");
                    });
                    Observable.Timer(TimeSpan.FromSeconds(39)).Subscribe(o =>
                    {
                        session.SendPacket("say 1 -1 10 Dans 1 secondes Le raid de la Ferme des moutons commence bientôt.");

                    });
                    Observable.Timer(TimeSpan.FromSeconds(50)).Subscribe(o =>
                    {
                        _map.Broadcast("srlst 3");
                        _map.Broadcast("sh_o");
                    });
                    Observable.Timer(TimeSpan.FromMinutes(5)).Subscribe(o =>
                    {

                        End(session.Character);
                    });
                }

                int i = 0;

                while (_map?.Sessions?.Any() == true)
                {
                    runRound(i++);
                }

                //ended
            }

            private static IEnumerable<Tuple<short, int, short, short>> generateDrop(Map.Map map, short vnum, int amountofdrop, int amount)
            {
                List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
                for (int i = 0; i < amountofdrop; i++)
                {
                    MapCell cell = map.GetRandomPosition();
                    dropParameters.Add(new Tuple<short, int, short, short>(vnum, amount, cell.X, cell.Y));
                }
                return dropParameters;
            }

            private static void removeSP(ClientSession session, short vnum)
            {
                if (session?.HasSession == true && !session.Character.IsVehicled)
                {
                    List<BuffType> bufftodisable = new List<BuffType> { BuffType.Bad, BuffType.Good, BuffType.Neutral };
                    session.Character.DisableBuffs(bufftodisable);
                    //session.Character.EquipmentBCards.RemoveAll(s => s.ItemVNum.Equals(vnum));
                    session.Character.UseSp = false;
                    session.Character.LoadSpeed();
                    session.SendPacket(session.Character.GenerateCond());
                    session.SendPacket(session.Character.GenerateLev());
                    session.Character.SpCooldown = 30;
                    if (session.Character.SkillsSp != null)
                    {
                        foreach (CharacterSkill ski in session.Character.SkillsSp.Values.Where(s => !s.CanBeUsed()))
                        {
                            short time = ski.Skill.Cooldown;
                            double temp = (ski.LastUse - DateTime.Now).TotalMilliseconds + (time * 100);
                            temp /= 1000;
                            session.Character.SpCooldown = temp > session.Character.SpCooldown ? (int)temp : session.Character.SpCooldown;
                        }
                    }
                    session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("STAY_TIME"), session.Character.SpCooldown), 11));
                    session.SendPacket($"sd {session.Character.SpCooldown}");
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                    session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(6, 1, session.Character.CharacterId), session.Character.PositionX, session.Character.PositionY);

                    // ms_c
                    session.SendPacket(session.Character.GenerateSki());
                    session.SendPackets(session.Character.GenerateQuicklist());
                    session.SendPacket(session.Character.GenerateStat());
                    session.SendPacket(session.Character.GenerateStatChar());

                    Observable.Timer(TimeSpan.FromMilliseconds(session.Character.SpCooldown * 1000)).Subscribe(o =>
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPPEAR"), 11));
                        session.SendPacket("sd 0");
                    });
                }
            }

            private void End(Character character)
            {
                //character.IsWaitingForGift = true;
                //character.Point = 0;
                //character.Point2 = 0;
                //character.Point3 = 0;
                //character.CanAttack = false;
                character.IsCustomSpeed = false;
                character.RemoveVehicle();
                Spawn = false;
                _map.Broadcast("srlst 4");
                _map.Broadcast("srlst 1");
            }

            private void runRound(int number)
            {
                int amount = 120 + (60 * number);

                int i = amount;
                while (i != 0)
                {
                    SpawnSheep(number);
                    Thread.Sleep(60000 / amount);
                    i--;
                }
                Thread.Sleep(70 * 100);
            }

            private void SpawnSheep(int round)
            {
                if (_map != null)
                {
                    if (Spawn == true)
                    {
                        MapCell cell = _map.Map.GetRandomPosition();

                        int SheepId = _map.GetNextId();

                        MapMonster sheep = new MapMonster { MonsterVNum = 9, MapX = cell.X, MapY = cell.Y, MapMonsterId = SheepId, IsHostile = false, IsMoving = true, ShouldRespawn = false };
                        sheep.Initialize(_map);
                        //sheep.NoAggresiveIcon = true;
                        _map.AddMonster(sheep);
                        _map.Broadcast(sheep.GenerateIn());
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}
