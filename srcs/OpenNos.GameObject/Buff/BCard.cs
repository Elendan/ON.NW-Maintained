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
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.GameObject.Battle;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;

namespace OpenNos.GameObject.Buff
{
    public class BCard : BCardDTO
    {
        #region Methods

        public void ApplyBCards(IBattleEntity session, IBattleEntity caster = null)
        {
            Mate mate = session is Mate ? (Mate)session.GetSession() : null;
            Character character = session is Character ? (Character)session.GetSession() : null;
            switch ((BCardType.CardType)Type)
            {
                case BCardType.CardType.Buff:
                    if (ServerManager.Instance.RandomNumber() < FirstData)
                    {
                        session?.BattleEntity.AddBuff(new Buff(SecondData,
                            caster?.BattleEntity.Level ?? session.BattleEntity.Level, entity: caster));
                    }

                    break;

                case BCardType.CardType.Move:
                    if (character == null)
                    {
                        break;
                    }

                    character.LastSpeedChange = DateTime.Now;
                    character.LoadSpeed();
                    character.Session.SendPacket(character.GenerateCond());
                    break;

                case BCardType.CardType.Summons:
                    NpcMonster npcMonster = session.GetSession() is MapMonster mob ? mob.Monster :
                        session.GetSession() is MapNpc npc ? npc.Npc : null;
                    ConcurrentBag<ToSummon> summonParameters = new ConcurrentBag<ToSummon>();

                    switch ((AdditionalTypes.Summons)SubType)
                    {
                        case AdditionalTypes.Summons.Summons:
                            for (int i = 0; i < FirstData; i++)
                            {
                                MapCell cell = session.GetPos();
                                cell.Y += (short)ServerManager.Instance.RandomNumber(-3, 3);
                                cell.X += (short)ServerManager.Instance.RandomNumber(-3, 3);
                                summonParameters.Add(new ToSummon((short)SecondData, cell, null, true,
                                    (byte)Math.Abs(ThirdData)));
                            }

                            EventHelper.Instance.RunEvent(new EventContainer(session.MapInstance,
                                EventActionType.SPAWNMONSTERS, summonParameters));
                            break;

                        case AdditionalTypes.Summons.SummonTrainingDummy:
                            if (npcMonster != null && session.BattleEntity.OnHitEvents.All(s =>
                                s?.EventActionType != EventActionType.SPAWNMONSTERS))
                            {
                                summonParameters.Add(new ToSummon((short)SecondData, session.GetPos(), null, true,
                                    (byte)Math.Abs(ThirdData)));
                                session.BattleEntity.OnHitEvents.Add(new EventContainer(session.MapInstance,
                                    EventActionType.SPAWNMONSTERS, summonParameters));
                            }

                            break;

                        case AdditionalTypes.Summons.SummonUponDeathChance:
                        case AdditionalTypes.Summons.SummonUponDeath:
                            if (npcMonster != null &&
                                session.BattleEntity.OnDeathEvents.All(s =>
                                    s?.EventActionType != EventActionType.SPAWNMONSTERS))
                            {
                                for (int i = 0; i < FirstData; i++)
                                {
                                    MapCell cell = session.GetPos();
                                    cell.Y += (short)i;
                                    summonParameters.Add(new ToSummon((short)SecondData, cell, null, true,
                                        (byte)Math.Abs(ThirdData)));
                                }

                                session.BattleEntity.OnDeathEvents.Add(new EventContainer(session.MapInstance,
                                    EventActionType.SPAWNMONSTERS, summonParameters));
                            }

                            break;

                        default:
                            break;
                    }

                    break;

                case BCardType.CardType.SpecialAttack:
                    break;

                case BCardType.CardType.SpecialDefence:
                    break;

                case BCardType.CardType.AttackPower:
                    break;

                case BCardType.CardType.Target:
                    break;

                case BCardType.CardType.Critical:
                    break;

                case BCardType.CardType.SpecialCritical:
                    break;

                case BCardType.CardType.Element:
                    break;

                case BCardType.CardType.IncreaseDamage:
                    break;

                case BCardType.CardType.Defence:
                    break;

                case BCardType.CardType.DodgeAndDefencePercent:
                    break;

                case BCardType.CardType.Block:
                    break;

                case BCardType.CardType.Absorption:
                    break;

                case BCardType.CardType.ElementResistance:
                    break;

                case BCardType.CardType.EnemyElementResistance:
                    break;

                case BCardType.CardType.Damage:
                    break;

                case BCardType.CardType.GuarantedDodgeRangedAttack:
                    break;

                case BCardType.CardType.Morale:
                    break;

                case BCardType.CardType.Casting:
                    break;

                case BCardType.CardType.Reflection:
                    break;

                case BCardType.CardType.DrainAndSteal:
                    if (ServerManager.Instance.RandomNumber() < FirstData)
                    {
                        return;
                    }
                    switch (SubType)
                    {
                        case (byte)AdditionalTypes.DrainAndSteal.LeechEnemyHP:
                            switch (session)
                            {
                                case MapMonster toDrain when caster is Character drainer:
                                {
                                    int heal = drainer.Level * SecondData;
                                    drainer.Hp = (int)(heal + drainer.Hp > drainer.HpLoad() ? drainer.HpLoad() : drainer.Hp + heal);
                                    drainer.MapInstance.Broadcast(drainer.GenerateRc((int)(heal + drainer.Hp > drainer.HpLoad() ? drainer.HpLoad() - drainer.Hp : heal)));
                                    toDrain.CurrentHp -= heal;
                                    drainer.MapInstance.Broadcast(drainer.GenerateStat());
                                    if (toDrain.CurrentHp <= 0)
                                    {
                                        toDrain.IsAlive = false;
                                        toDrain.GenerateDeath(drainer);
                                    }

                                    break;
                                }
                                case Character characterDrained when caster is Character drainerCharacter:
                                {
                                    int heal = drainerCharacter.Level * SecondData;
                                    drainerCharacter.Hp = (int)(heal + drainerCharacter.Hp > drainerCharacter.HpLoad() ? drainerCharacter.HpLoad() : drainerCharacter.Hp + heal);
                                    drainerCharacter.MapInstance.Broadcast(drainerCharacter.GenerateRc((int)(heal + drainerCharacter.Hp > drainerCharacter.HpLoad() ? drainerCharacter.HpLoad() - drainerCharacter.Hp : heal)));
                                    characterDrained.Hp -= heal;
                                    characterDrained.MapInstance.Broadcast(characterDrained.GenerateStat());
                                    drainerCharacter.MapInstance.Broadcast(drainerCharacter.GenerateStat());
                                        if (characterDrained.Hp <= 0)
                                    {
                                        characterDrained.GenerateDeath(drainerCharacter);
                                        ServerManager.Instance.AskRevive(characterDrained.CharacterId, drainerCharacter.Session);
                                    }

                                    break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    break;

                case BCardType.CardType.HealingBurningAndCasting:
                    var subtype = (AdditionalTypes.HealingBurningAndCasting)SubType;
                    switch (subtype)
                    {
                        case AdditionalTypes.HealingBurningAndCasting.RestoreHP:
                        case AdditionalTypes.HealingBurningAndCasting.RestoreHPWhenCasting:
                            if (character != null)
                            {
                                int heal = FirstData;
                                bool change = false;
                                if (IsLevelScaled)
                                {
                                    if (IsLevelDivided)
                                    {
                                        heal /= character.Level;
                                    }
                                    else
                                    {
                                        heal *= character.Level;
                                    }
                                }

                                if (character.Hp + heal < character.HpLoad())
                                {
                                    character.Hp += heal;
                                    character.Session?.CurrentMapInstance?.Broadcast(character.GenerateRc(heal));
                                    change = true;
                                }
                                else
                                {
                                    if (character.Hp != (int)character.HpLoad())
                                    {
                                        character.Session?.CurrentMapInstance?.Broadcast(
                                            character.GenerateRc((int)(character.HpLoad() - character.Hp)));
                                        change = true;
                                    }

                                    character.Hp = (int)character.HpLoad();
                                }

                                if (change)
                                {
                                    character.Session?.SendPacket(character.GenerateStat());
                                }
                            }

                            if (mate != null)
                            {
                                int heal = FirstData;
                                if (IsLevelScaled)
                                {
                                    if (IsLevelDivided)
                                    {
                                        heal /= mate.Level;
                                    }
                                    else
                                    {
                                        heal *= mate.Level;
                                    }
                                }

                                if (mate.Hp + heal < mate.HpLoad())
                                {
                                    mate.Hp += heal;
                                }
                                else
                                {
                                    mate.Hp = mate.HpLoad();
                                }
                            }

                            break;
                        case AdditionalTypes.HealingBurningAndCasting.RestoreMP:
                            if (character != null)
                            {
                                int heal = FirstData;
                                bool change = false;
                                if (IsLevelScaled)
                                {
                                    if (IsLevelDivided)
                                    {
                                        heal /= character.Level;
                                    }
                                    else
                                    {
                                        heal *= character.Level;
                                    }
                                }

                                if (character.Mp + heal < character.MpLoad())
                                {
                                    character.Mp += heal;
                                    change = true;
                                }
                                else
                                {
                                    if (character.Mp != (int)character.MpLoad())
                                    {
                                        change = true;
                                    }

                                    character.Mp = (int)character.MpLoad();
                                }

                                if (change)
                                {
                                    character.Session?.SendPacket(character.GenerateStat());
                                }
                            }

                            if (mate != null)
                            {
                                int heal = FirstData;
                                if (IsLevelScaled)
                                {
                                    if (IsLevelDivided)
                                    {
                                        heal /= mate.Level;
                                    }
                                    else
                                    {
                                        heal *= mate.Level;
                                    }
                                }

                                if (mate.Mp + heal < mate.MpLoad())
                                {
                                    mate.Mp += heal;
                                }
                                else
                                {
                                    mate.Mp = mate.MpLoad();
                                }
                            }

                            break;
                        case AdditionalTypes.HealingBurningAndCasting.DecreaseHP:
                            int timer = ThirdData + 1;
                            IDisposable obs = null;
                            Card card = ServerManager.Instance.GetCardByCardId(CardId ?? -1);
                            if (card == null)
                            {
                                Logger.Log.Warn("CardId was null, can't apply bcard.");
                                return;
                            }
                            if (IsLevelScaled)
                            {
                                int scale = FirstData + 1;
                                ushort damage = 0;
                                obs = Observable.Interval(TimeSpan.FromSeconds(timer)).Subscribe(s =>
                                {
                                    switch (session)
                                    {
                                        case Character receiverCharacter when caster is Character senderCharacter:
                                            {
                                                damage = (ushort)(senderCharacter.Level * scale);
                                                receiverCharacter.Hp = receiverCharacter.Hp - damage <= 0 ? 1 : receiverCharacter.Hp - damage;
                                                receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateDm(damage));
                                                receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateStat());
                                                break;
                                            }
                                        case MapMonster receiverMonster when caster is Character senderCharacter:
                                            {
                                                damage = (ushort)(senderCharacter.Level * scale);
                                                receiverMonster.CurrentHp = receiverMonster.CurrentHp - damage <= 0 ? 1 : receiverMonster.CurrentHp - damage;
                                                receiverMonster.MapInstance.Broadcast(receiverMonster.GenerateDm(damage));
                                            }
                                            break;
                                        case Character receiverCharacter when caster is MapMonster senderMapMonster:
                                            {
                                                damage = (ushort)(senderMapMonster.Monster.Level * scale);
                                                receiverCharacter.Hp = receiverCharacter.Hp - damage <= 0 ? 1 : receiverCharacter.Hp - damage;
                                                receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateDm(damage));
                                                receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateStat());
                                            }
                                            break;
                                    }
                                });
                                Observable.Timer(TimeSpan.FromSeconds(card.Duration * 0.1)).Subscribe(s =>
                                {
                                    obs.Dispose();
                                });
                            }
                            else
                            {
                                ushort damage = (ushort)FirstData;
                                obs = Observable.Interval(TimeSpan.FromSeconds(timer)).Subscribe(s =>
                                {
                                    switch (session)
                                    {
                                        case Character receiverCharacter when caster is Character senderCharacter:
                                            damage = senderCharacter.Level;
                                            receiverCharacter.Hp = receiverCharacter.Hp - damage <= 0 ? 1 : receiverCharacter.Hp - damage;
                                            receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateDm(damage));
                                            receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateStat());
                                            break;
                                        case MapMonster receiverMonster when caster is Character senderCharacter:
                                            damage = senderCharacter.Level;
                                            receiverMonster.CurrentHp = receiverMonster.CurrentHp - damage <= 0 ? 1 : receiverMonster.CurrentHp - damage;
                                            receiverMonster.MapInstance.Broadcast(receiverMonster.GenerateDm(damage));
                                            break;
                                        case Character receiverCharacter when caster is MapMonster senderMapMonster:
                                            damage = senderMapMonster.Monster.Level;
                                            receiverCharacter.Hp = receiverCharacter.Hp - damage <= 0 ? 1 : receiverCharacter.Hp - damage;
                                            receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateDm(damage));
                                            receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateStat());
                                            break;
                                    }
                                });
                                Observable.Timer(TimeSpan.FromSeconds(card.Duration * 0.1)).Subscribe(s => obs.Dispose());
                            }
                            break;
                        case AdditionalTypes.HealingBurningAndCasting.DecreaseMP:
                            int mpTimer = ThirdData + 1;
                            IDisposable mpObs = null;
                            Card mpCard = ServerManager.Instance.GetCardByCardId(CardId ?? -1);
                            if (mpCard == null)
                            {
                                Logger.Log.Warn("CardId was null, can't apply bcard.");
                                return;
                            }
                            if (IsLevelScaled)
                            {
                                int scale = FirstData + 1;
                                ushort damage = 0;
                                obs = Observable.Interval(TimeSpan.FromSeconds(mpTimer)).Subscribe(s =>
                                {
                                    switch (session)
                                    {
                                        case Character receiverCharacter when caster is Character senderCharacter:
                                            damage = (ushort)(senderCharacter.Level * scale);
                                            receiverCharacter.Mp = receiverCharacter.Mp - damage <= 0 ? 1 : receiverCharacter.Mp - damage;
                                            receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateStat());
                                            break;
                                        case MapMonster receiverMonster when caster is Character senderCharacter:
                                            damage = (ushort)(senderCharacter.Level * scale);
                                            receiverMonster.CurrentMp = receiverMonster.CurrentMp - damage <= 0 ? 1 : receiverMonster.CurrentMp - damage;
                                            break;
                                        case Character receiverCharacter when caster is MapMonster senderMapMonster:
                                            damage = (ushort)(senderMapMonster.Monster.Level * scale);
                                            receiverCharacter.Mp = receiverCharacter.Mp - damage <= 0 ? 1 : receiverCharacter.Mp - damage;
                                            receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateStat());
                                            break;
                                    }
                                });
                                Observable.Timer(TimeSpan.FromSeconds(mpCard.Duration * 0.1)).Subscribe(s => obs.Dispose());
                            }
                            else
                            {
                                ushort damage = (ushort)FirstData;
                                obs = Observable.Interval(TimeSpan.FromSeconds(mpTimer)).Subscribe(s =>
                                {
                                    switch (session)
                                    {
                                        case Character receiverCharacter when caster is Character senderCharacter:
                                            damage = senderCharacter.Level;
                                            receiverCharacter.Mp = receiverCharacter.Mp - damage <= 0 ? 1 : receiverCharacter.Mp - damage;
                                            receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateStat());
                                            break;
                                        case MapMonster receiverMonster when caster is Character senderCharacter:
                                            damage = senderCharacter.Level;
                                            receiverMonster.CurrentMp = receiverMonster.CurrentMp - damage <= 0 ? 1 : receiverMonster.CurrentMp - damage;
                                            break;
                                        case Character receiverCharacter when caster is MapMonster senderMapMonster:
                                            damage = senderMapMonster.Monster.Level;
                                            receiverCharacter.Mp = receiverCharacter.Mp - damage <= 0 ? 1 : receiverCharacter.Mp - damage;
                                            receiverCharacter.MapInstance.Broadcast(receiverCharacter.GenerateStat());
                                            break;
                                    }
                                });
                                Observable.Timer(TimeSpan.FromSeconds(mpCard.Duration * 0.1)).Subscribe(s => obs.Dispose());
                            }
                            break;
                    }

                    break;

                case BCardType.CardType.HPMP:
                    break;

                case BCardType.CardType.SpecialisationBuffResistance:
                    break;

                case BCardType.CardType.SpecialEffects:
                    break;

                case BCardType.CardType.Capture:
                    if (session is MapMonster monsterToCapture && caster is Character hunter)
                    {
                        if (monsterToCapture.Monster.RaceType == 1 &&
                            (hunter.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance ||
                                hunter.MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance))
                        {
                            if (monsterToCapture.Monster.Level < hunter.Level)
                            {
                                if (monsterToCapture.CurrentHp < monsterToCapture.Monster.MaxHP / 2)
                                {
                                    if (hunter.MaxMateCount > hunter.Mates.Count())
                                    {
                                        // Algo  
                                        int capturerate =
                                            100 - (monsterToCapture.CurrentHp / monsterToCapture.Monster.MaxHP + 1) / 2;
                                        if (ServerManager.Instance.RandomNumber() <= capturerate)
                                        {
                                            if (hunter.Quests.Any(q =>
                                                q.Quest.QuestType == (int)QuestType.Capture1 &&
                                                q.Quest.QuestObjectives.Any(d =>
                                                    d.Data == monsterToCapture.MonsterVNum)))
                                            {
                                                hunter.IncrementQuests(QuestType.Capture1,
                                                    monsterToCapture.MonsterVNum);
                                                return;
                                            }

                                            hunter.IncrementQuests(QuestType.Capture2, monsterToCapture.MonsterVNum);
                                            int level = monsterToCapture.Monster.Level - 15 < 1
                                                ? 1
                                                : monsterToCapture.Monster.Level - 15;
                                            Mate currentmate = hunter.Mates?.FirstOrDefault(m =>
                                                m.IsTeamMember && m.MateType == MateType.Pet);
                                            if (currentmate != null)
                                            {
                                                currentmate.RemoveTeamMember(); // remove current pet
                                                hunter.MapInstance.Broadcast(currentmate.GenerateOut());
                                            }

                                            monsterToCapture.MapInstance.DespawnMonster(monsterToCapture);
                                            NpcMonster mateNpc =
                                                ServerManager.Instance.GetNpc(monsterToCapture.MonsterVNum);
                                            mate = new Mate(hunter, mateNpc, (byte)level, MateType.Pet);
                                            hunter.Mates?.Add(mate);
                                            mate.RefreshStats();
                                            hunter.Session.SendPacket($"ctl 2 {mate.PetId} 3");
                                            hunter.MapInstance.Broadcast(mate.GenerateIn());
                                            hunter.Session.SendPacket(hunter.GenerateSay(
                                                string.Format(Language.Instance.GetMessageFromKey("YOU_GET_PET"),
                                                    mate.Name), 0));
                                            hunter.Session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());
                                            hunter.Session.SendPackets(hunter.GenerateScP());
                                            hunter.Session.SendPackets(hunter.GenerateScN());
                                            hunter.Session.SendPacket(hunter.GeneratePinit());
                                            hunter.Session.SendPackets(hunter.Mates.Where(s => s.IsTeamMember)
                                                .OrderBy(s => s.MateType)
                                                .Select(s => s.GeneratePst()));
                                        }
                                        else
                                        {
                                            hunter.Session.SendPacket(
                                                UserInterfaceHelper.Instance.GenerateMsg(
                                                    Language.Instance.GetMessageFromKey("CAPTURE_FAILED"), 0));
                                        }
                                    }
                                    else
                                    {
                                        hunter.Session.SendPacket(
                                            UserInterfaceHelper.Instance.GenerateMsg(
                                                Language.Instance.GetMessageFromKey("MAX_MATES_COUNT"), 0));
                                    }
                                }
                                else
                                {
                                    hunter.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("monsterToCapture_MUST_BE_LOW_HP"), 0));
                                }
                            }
                            else
                            {
                                hunter.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("monsterToCapture_LVL_MUST_BE_LESS"), 0));
                            }
                        }
                        else
                        {
                            hunter.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(
                                Language.Instance.GetMessageFromKey("monsterToCapture_CANNOT_BE_CAPTURED"), 0));
                        }
                    }

                    break;

                case BCardType.CardType.SpecialDamageAndExplosions:
                    break;

                case BCardType.CardType.SpecialEffects2:
                    break;

                case BCardType.CardType.CalculatingLevel:
                    break;

                case BCardType.CardType.Recovery:
                    break;

                case BCardType.CardType.MaxHPMP:
                    break;

                case BCardType.CardType.MultAttack:
                    break;

                case BCardType.CardType.MultDefence:
                    break;

                case BCardType.CardType.TimeCircleSkills:
                    break;

                case BCardType.CardType.RecoveryAndDamagePercent:
                    break;

                case BCardType.CardType.Count:
                    break;

                case BCardType.CardType.NoDefeatAndNoDamage:
                    break;

                case BCardType.CardType.SpecialActions:
                    switch (SubType)
                    {
                        case (byte)AdditionalTypes.SpecialActions.Hide:
                            if (character == null)
                            {
                                break;
                            }
                            character.Invisible = true;
                            character.Mates.Where(s => s.IsTeamMember).ToList().ForEach(s =>
                                character.Session.CurrentMapInstance?.Broadcast(s.GenerateOut()));
                            character.Session.CurrentMapInstance?.Broadcast(character.GenerateInvisible());
                            break;
                        case (byte)AdditionalTypes.SpecialActions.FocusEnemies:
                            long entityId;
                            UserType uType;
                            switch (caster)
                            {
                                case Character senderCharacter:
                                    switch (session)
                                    {
                                        case MapMonster receiverMapMonster:
                                            entityId = receiverMapMonster.MapMonsterId;
                                            uType = UserType.Monster;
                                            break;
                                        case Character receiverCharacter:
                                            entityId = receiverCharacter.CharacterId;
                                            uType = UserType.Player;
                                            break;
                                        default:
                                            return;
                                    }

                                    Observable.Timer(TimeSpan.FromMilliseconds(500)).Subscribe(s =>
                                    {
                                        senderCharacter.MapInstance.Broadcast($"guri 3 {(short)uType} {entityId} {senderCharacter.PositionX} {senderCharacter.PositionY} 3 {SecondData} 2 -1");
                                    });
                                    break;
                                case MapMonster senderMapMonster:
                                    switch (session)
                                    {
                                        case MapMonster receiverMapMonster:
                                            entityId = receiverMapMonster.MapMonsterId;
                                            uType = UserType.Monster;
                                            break;
                                        case Character receiverCharacter:
                                            entityId = receiverCharacter.CharacterId;
                                            uType = UserType.Player;
                                            break;
                                        default:
                                            return;
                                    }
                                    Observable.Timer(TimeSpan.FromMilliseconds(500)).Subscribe(s =>
                                    {
                                        senderMapMonster.MapInstance.Broadcast($"guri 3 {(short)uType} {entityId} {senderMapMonster.MapX} {senderMapMonster.MapY} 3 {SecondData} 2 -1");
                                    });
                                    break;
                            }

                            break;
                    }

                    break;

                case BCardType.CardType.Mode:
                    break;

                case BCardType.CardType.NoCharacteristicValue:
                    break;

                case BCardType.CardType.LightAndShadow:
                    break;

                case BCardType.CardType.Item:
                    break;

                case BCardType.CardType.DebuffResistance:
                    break;

                case BCardType.CardType.SpecialBehaviour:
                    switch (SubType)
                    {
                        case (byte)AdditionalTypes.SpecialBehaviour.InflictOnTeam:
                            int delay = ThirdData + 1;
                            IDisposable teamObs = null;
                            if (session is MapMonster inRangeMapMonster)
                            {
                                int range = FirstData;
                                int timer = ThirdData + 1;
                                Card buffCard = ServerManager.Instance.GetCardByCardId((short)SecondData);
                                IEnumerable entitiesInRange = inRangeMapMonster.MapInstance.GetListMonsterInRange(inRangeMapMonster.MapX, inRangeMapMonster.MapY, (byte)range);
                                if (entitiesInRange == null || buffCard == null)
                                {
                                    return;
                                }

                                teamObs = Observable.Interval(TimeSpan.FromSeconds(timer)).Subscribe(s =>
                                {
                                    foreach (MapMonster monster in entitiesInRange)
                                    {
                                        if (monster.Buffs.All(x => x.Card.CardId != buffCard.CardId))
                                        {
                                            monster.AddBuff(new Buff(SecondData, entity: caster));
                                        }
                                    }
                                });

                                Observable.Timer(TimeSpan.FromSeconds(buffCard.Duration * 0.1)).Subscribe(s =>
                                {
                                    teamObs.Dispose();
                                });
                            }
                            else if (session is Character inRangeCharacter)
                            {
                                int range = FirstData;
                                int timer = ThirdData + 1;
                                Card buffCard = ServerManager.Instance.GetCardByCardId((short)SecondData);
                                IEnumerable entitiesInRange = inRangeCharacter.MapInstance.GetCharactersInRange(inRangeCharacter.MapX, inRangeCharacter.MapY, (byte)range);
                                if (entitiesInRange == null || buffCard == null)
                                {
                                    return;
                                }

                                teamObs = Observable.Interval(TimeSpan.FromSeconds(timer)).Subscribe(s =>
                                {
                                    foreach (Character characterInRange in entitiesInRange)
                                    {
                                        if (characterInRange.Buff.All(x => x.Card.CardId != buffCard.CardId))
                                        {
                                            characterInRange.AddBuff(new Buff(SecondData, entity: caster));
                                        }
                                    }
                                });

                                Observable.Timer(TimeSpan.FromSeconds(buffCard.Duration * 0.1)).Subscribe(s =>
                                {
                                    teamObs.Dispose();
                                });
                            }
                            break;
                    }
                    break;

                case BCardType.CardType.Quest:
                    break;

                case BCardType.CardType.SecondSPCard:
                    break;

                case BCardType.CardType.SPCardUpgrade:
                    break;

                case BCardType.CardType.HugeSnowman:
                    break;

                case BCardType.CardType.Drain:
                    break;

                case BCardType.CardType.BossMonstersSkill:
                    break;

                case BCardType.CardType.LordHatus:
                    break;

                case BCardType.CardType.LordCalvinas:
                    break;

                case BCardType.CardType.SESpecialist:
                    break;

                case BCardType.CardType.FourthGlacernonFamilyRaid:
                    break;

                case BCardType.CardType.SummonedMonsterAttack:
                    break;

                case BCardType.CardType.BearSpirit:
                    break;

                case BCardType.CardType.SummonSkill:
                    break;

                case BCardType.CardType.InflictSkill:
                    break;

                case BCardType.CardType.HideBarrelSkill:
                    break;

                case BCardType.CardType.FocusEnemyAttentionSkill:
                    break;

                case BCardType.CardType.TauntSkill:
                    break;

                case BCardType.CardType.FireCannoneerRangeBuff:
                    break;

                case BCardType.CardType.VulcanoElementBuff:
                    break;

                case BCardType.CardType.DamageConvertingSkill:
                    break;

                case BCardType.CardType.MeditationSkill:
                    if (session.GetSession().GetType() == typeof(Character))
                    {
                        if (SubType.Equals((byte)AdditionalTypes.MeditationSkill.CausingChance))
                        {
                            if (ServerManager.Instance.RandomNumber() < FirstData)
                            {
                                if (character == null)
                                {
                                    break;
                                }

                                if (SkillVNum.HasValue)
                                {
                                    Skill skill = ServerManager.Instance.GetSkill(SkillVNum.Value);
                                    Skill newSkill = ServerManager.Instance.GetSkill((short)SecondData);
                                    Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(observer =>
                                    {
                                        foreach (QuicklistEntryDTO quicklistEntry in character.QuicklistEntries.Where(s => s.Pos.Equals(skill.CastId)))
                                        {
                                            character.Session.SendPacket($"qset {quicklistEntry.Q1} {quicklistEntry.Q2} {quicklistEntry.Type}.{quicklistEntry.Slot}.{newSkill.CastId}.0");
                                        }
                                        character.Session.SendPacket($"mslot {newSkill.CastId} -1");
                                    });
                                    character.SkillComboCount++;
                                    character.LastSkillCombo = DateTime.Now;
                                    if (skill.CastId > 10)
                                    {
                                        // HACK this way
                                        Observable.Timer(TimeSpan.FromMilliseconds(skill.Cooldown * 100 + 500))
                                            .Subscribe(observer => { character.Session.SendPacket($"sr {skill.CastId}"); });
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (character == null)
                            {
                                break;
                            }

                            switch (SubType)
                            {
                                case 21:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.Now.AddSeconds(4);
                                    break;
                                case 31:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.Now.AddSeconds(8);
                                    break;
                                case 41:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.Now.AddSeconds(12);
                                    break;
                            }
                        }
                    }

                    break;

                case BCardType.CardType.FalconSkill:
                    if (character == null)
                    {
                        break;
                    }

                    switch (SubType)
                    {
                        case (byte)AdditionalTypes.FalconSkill.Hide:
                            character.Invisible = true;
                            character.Mates.Where(s => s.IsTeamMember).ToList().ForEach(s =>
                                character.Session.CurrentMapInstance?.Broadcast(s.GenerateOut()));
                            character.Session.CurrentMapInstance?.Broadcast(character.GenerateInvisible());
                            break;
                    }

                    break;

                case BCardType.CardType.AbsorptionAndPowerSkill:
                    break;

                case BCardType.CardType.LeonaPassiveSkill:
                    break;

                case BCardType.CardType.FearSkill:
                    break;

                case BCardType.CardType.SniperAttack:
                    break;

                case BCardType.CardType.FrozenDebuff:
                    break;

                case BCardType.CardType.JumpBackPush:
                    break;

                case BCardType.CardType.FairyXPIncrease:
                    break;

                case BCardType.CardType.SummonAndRecoverHP:
                    break;

                case BCardType.CardType.TeamArenaBuff:
                    break;

                case BCardType.CardType.ArenaCamera:
                    break;

                case BCardType.CardType.DarkCloneSummon:
                    break;

                case BCardType.CardType.AbsorbedSpirit:
                    break;

                case BCardType.CardType.AngerSkill:
                    break;

                case BCardType.CardType.MeteoriteTeleport:
                    break;

                case BCardType.CardType.StealBuff:
                    break;

                case BCardType.CardType.EffectSummon:
                    break;

                default:
                    Logger.Error(new ArgumentOutOfRangeException($"Card Type {Type} not defined!"));
                    //throw new ArgumentOutOfRangeException();
                    break;
            }
        }

        public override void Initialize()
        {
        }

        #endregion
    }
}