using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CloneExtensions;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.ACT4
{
    public class Act4Raid
    {
        #region Methods

        public List<MapMonster> Act4Guardians { get; set; }

        public void GenerateRaid(byte type, byte faction)
        {
            Act4Guardians = new List<MapMonster>();
            ScriptedInstance raid = ServerManager.Instance.Act4Raids.FirstOrDefault(r => r.Id == type);
            MapInstance lobby = ServerManager.Instance.Act4Maps.FirstOrDefault(m => m.Map.MapId == 134);

            if (raid == null || lobby == null)
            {
                Logger.Log.Error(raid == null ? $"Act4 raids is missing - type : {type}" : "There is no map in Act4Maps with MapId == 134");
                return;
            }

            ServerManager.Instance.Act4RaidStart = DateTime.Now;

            lobby.CreatePortal(new Portal
            {
                SourceMapId = 134,
                SourceX = 139,
                SourceY = 100,
                Type = (short)(9 + faction)
            }, 3600, true);

            #region Guardian Spawning

            Act4Guardians.Add(new MapMonster
            {
                MonsterVNum = (short)(678 + faction),
                MapX = 147,
                MapY = 88,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = lobby.GetNextId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Act4Guardians.Add(new MapMonster
            {
                MonsterVNum = (short)(678 + faction),
                MapX = 149,
                MapY = 94,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = lobby.GetNextId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Act4Guardians.Add(new MapMonster
            {
                MonsterVNum = (short)(678 + faction),
                MapX = 147,
                MapY = 101,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = lobby.GetNextId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Act4Guardians.Add(new MapMonster
            {
                MonsterVNum = (short)(678 + faction),
                MapX = 139,
                MapY = 105,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = lobby.GetNextId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Act4Guardians.Add(new MapMonster
            {
                MonsterVNum = (short)(678 + faction),
                MapX = 132,
                MapY = 101,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = lobby.GetNextId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Act4Guardians.Add(new MapMonster
            {
                MonsterVNum = (short)(678 + faction),
                MapX = 129,
                MapY = 94,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = lobby.GetNextId(),
                ShouldRespawn = false,
                IsHostile = true
            });
            Act4Guardians.Add(new MapMonster
            {
                MonsterVNum = (short)(678 + faction),
                MapX = 132,
                MapY = 88,
                MapId = 134,
                Position = 2,
                IsMoving = false,
                MapMonsterId = lobby.GetNextId(),
                ShouldRespawn = false,
                IsHostile = true
            });

            foreach (MapMonster monster in Act4Guardians)
            {
                monster.Initialize();
                lobby.AddMonster(monster);
                lobby.Broadcast(monster.GenerateIn());
            }

            Observable.Timer(TimeSpan.FromMinutes(60)).Subscribe(s =>
            {
                foreach (MapMonster monster in Act4Guardians)
                {
                    lobby.RemoveMonster(monster);
                    lobby.Broadcast(monster.GenerateOut());
                }
                Act4Guardians.Clear();
            });

            #endregion

            foreach (MapInstance map in ServerManager.Instance.Act4Maps)
            {
                map.Sessions.Where(s => s?.Character?.Faction == (FactionType)faction).ToList().ForEach(s =>
                    s.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("ACT4_RAID_OPEN"), ((Act4RaidType)type).ToString()), 0)));
            }

            lock (ServerManager.Instance.FamilyList)
            {
                foreach (Family family in ServerManager.Instance.FamilyList.GetAllItems())
                {
                    if (family == null)
                    {
                        continue;
                    }
                    family.Act4Raid = ServerManager.Instance.Act4Raids.FirstOrDefault(r => r.Id == type)?.GetClone();
                    family.Act4Raid?.LoadScript(MapInstanceType.RaidInstance);
                    if (family.Act4Raid?.FirstMap == null)
                    {
                        continue;
                    }
                    family.Act4Raid.FirstMap.InstanceBag.Lock = true;
                }
            }

            Observable.Timer(TimeSpan.FromSeconds(3600)).Subscribe(s =>
            {
                foreach (Family family in ServerManager.Instance.FamilyList.GetAllItems())
                {
                    if (family == null)
                    {
                        continue;
                    }
                    family.Act4Raid?.MapInstanceDictionary?.Values.ToList().ForEach(m => m?.Dispose());
                    family.Act4Raid = null;
                }
            });
        }

        #endregion

        #region Singleton

        private static Act4Raid _instance;

        public static Act4Raid Instance => _instance ?? (_instance = new Act4Raid());

        #endregion
    }
}