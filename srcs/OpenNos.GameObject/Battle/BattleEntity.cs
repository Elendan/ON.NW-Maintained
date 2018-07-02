using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Core.Networking.Communication.Scs.Threading;
using OpenNos.Data;
using OpenNos.GameObject.Battle.Args;
using OpenNos.GameObject.Buff;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using static NosSharp.Enums.BCardType;

namespace OpenNos.GameObject.Battle
{
    public class BattleEntity
    {
        #region instantiation

        public BattleEntity(IBattleEntity entity)
        {
            Entity = entity;
            Session = entity.GetSession();
            Buffs = new ConcurrentBag<Buff.Buff>();
            StaticBcards = new ConcurrentBag<BCard>();
            SkillBcards = new ConcurrentBag<BCard>();
            OnDeathEvents = new ConcurrentBag<EventContainer>();
            OnHitEvents = new ConcurrentBag<EventContainer>();
            ObservableBag = new ConcurrentDictionary<short, IDisposable>();
            ShellOptionArmor = new ConcurrentBag<EquipmentOptionDTO>();
            ShellOptionsMain = new ConcurrentBag<EquipmentOptionDTO>();
            ShellOptionsSecondary = new ConcurrentBag<EquipmentOptionDTO>();
            CostumeHatBcards = new ConcurrentBag<BCard>();
            CostumeSuitBcards = new ConcurrentBag<BCard>();
            CostumeBcards = new ConcurrentBag<BCard>();
            CellonOptions = new ThreadSafeGenericList<EquipmentOptionDTO>();

            if (Session is Character character)
            {
                Level = character.Level;
                return;
            }

            NpcMonster npcMonster = Session is MapMonster mon ? mon.Monster
                : Session is MapNpc npc ? npc.Npc
                : Session is Mate mate ? mate.Monster
                : null;

            if (npcMonster == null)
            {
                return;
            }

            Level = npcMonster.Level;
            Element = npcMonster.Element;
            ElementRate = npcMonster.ElementRate;
            FireResistance = npcMonster.FireResistance;
            WaterResistance = npcMonster.WaterResistance;
            LightResistance = npcMonster.LightResistance;
            DarkResistance = npcMonster.DarkResistance;
            DefenceRate = npcMonster.DefenceDodge;
            DistanceDefenceRate = npcMonster.DistanceDefenceDodge;
            CloseDefence = npcMonster.CloseDefence;
            RangedDefence = npcMonster.DistanceDefence;
            MagicDefence = npcMonster.MagicDefence;
            AttackUpgrade = npcMonster.AttackUpgrade;
            CriticalChance = npcMonster.CriticalChance;
            CriticalRate = npcMonster.CriticalRate - 30;
            MinDamage = npcMonster.DamageMinimum;
            MaxDamage = npcMonster.DamageMaximum;
            HitRate = npcMonster.Concentrate;
        }

        #endregion

        public event EventHandler<HitArgs> Hit;
        public event EventHandler<KillArgs> Kill;
        public event EventHandler<DeathArgs> Death;
        public event EventHandler<MoveArgs> Move;

        public virtual void OnMove(MoveArgs args)
        {
            Move?.Invoke(this, args);
        }

        public virtual void OnDeath(DeathArgs args)
        {
            Death?.Invoke(this, args);
        }

        public virtual void OnKill(KillArgs args)
        {
            Kill?.Invoke(this, args);
        }

        public virtual void OnHit(HitArgs e)
        {
            Hit?.Invoke(this, e);
        }

        #region Porperties

        public AttackType AttackType { get; set; }

        public ConcurrentBag<EquipmentOptionDTO> ShellOptionsMain { get; set; }

        public ConcurrentBag<EquipmentOptionDTO> ShellOptionsSecondary { get; set; }

        public ConcurrentBag<EquipmentOptionDTO> ShellOptionArmor { get; set; }

        public ConcurrentBag<BCard> CostumeSuitBcards { get; set; }

        public ConcurrentBag<BCard> CostumeHatBcards { get; set; }

        public ConcurrentBag<BCard> CostumeBcards { get; set; }

        public ThreadSafeGenericList<EquipmentOptionDTO> CellonOptions { get; set; }

        public ConcurrentBag<Buff.Buff> Buffs { get; set; }

        public ConcurrentBag<BCard> StaticBcards { get; set; }

        public int HpMax { get; set; }

        public int Morale { get; set; }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public int MpMax { get; set; }

        public int Resistance { get; set; }

        public EntityType EntityType { get; set; }

        public ConcurrentBag<BCard> SkillBcards { get; set; }

        public ConcurrentBag<EventContainer> OnDeathEvents { get; set; }

        public ConcurrentBag<EventContainer> OnHitEvents { get; set; }

        private ConcurrentDictionary<short, IDisposable> ObservableBag { get; }

        public object Session { get; set; }

        public IBattleEntity Entity { get; set; }

        public byte Level { get; set; }

        public bool IsReflecting { get; set; }

        #region Element

        public byte Element { get; set; }

        public int ElementRate { get; set; }

        public int ElementRateSp { get; set; }

        public int FireResistance { get; set; }

        public int WaterResistance { get; set; }

        public int LightResistance { get; set; }

        public int DarkResistance { get; set; }

        #endregion

        #region Attack
        public int ChargeValue { get; set; }

        public short AttackUpgrade { get; set; }

        public int CriticalChance { get; set; }

        public int CriticalRate { get; set; }

        public int MinDamage { get; set; }

        public int MaxDamage { get; set; }

        public int WeaponDamageMinimum { get; set; }

        public int WeaponDamageMaximum { get; set; }

        public int HitRate { get; set; }

        public int DefenseUpgrade { get; set; }

        public int ArmorMeleeDefense { get; set; }

        public int ArmorRangeDefense { get; set; }

        public int ArmorMagicalDefense { get; set; }

        public int MeleeDefense { get; set; }

        public int MeleeDefenseDodge { get; set; }

        public int RangeDefense { get; set; }

        public int RangeDefenseDodge { get; set; }

        public int MagicalDefense { get; set; }

        public int Defense { get; set; }

        public int ArmorDefense { get; set; }

        public int Dodge { get; set; }

        #endregion

        #region Defence

        public short DefenceUpgrade { get; set; }

        public int DefenceRate { get; set; }

        public int DistanceDefenceRate { get; set; }

        public int CloseDefence { get; set; }

        public int RangedDefence { get; set; }

        public int MagicDefence { get; set; }

        #endregion

        #endregion

        #region Methods

        public int RandomTimeBuffs(Buff.Buff indicator)
        {
            if (Session is Character character)
            {
                switch (indicator.Card.CardId)
                {
                    //SP2a invisibility
                    case 85:
                        return ServerManager.Instance.RandomNumber(50, 350);
                    // SP6a invisibility
                    case 559:
                        return 350;
                    // Speed booster
                    case 336:
                        return ServerManager.Instance.RandomNumber(30, 70);
                    // Charge buff types
                    case 0:
                        return character.ChargeValue > 7000 ? 7000 : character.ChargeValue;
                    //Imp hat
                    case 2154:
                    case 2155:
                    case 2156:
                    case 2157:
                    case 2158:
                    case 2159:
                    case 2160:
                        return ServerManager.Instance.RandomNumber(100, 200);
                    case 319:
                        return 600;
                }
            }
            return -1;
        }

        public void AddBuff(Buff.Buff indicator, IBattleEntity caster = null)
        {
            if (indicator?.Card == null || indicator.Card.BuffType == BuffType.Bad &&
                Buffs.Any(b => b.Card.CardId == indicator.Card.CardId))
            {
                return;
            }

            //TODO: add a scripted way to remove debuffs from boss when a monster is killed (475 is laurena's buff)
            if (indicator.Card.CardId == 475)
            {
                return;
            }

            Buffs.RemoveWhere(s => !s.Card.CardId.Equals(indicator.Card.CardId), out ConcurrentBag<Buff.Buff> buffs);
            Buffs = buffs;
            int randomTime = 0;
            if (Session is Character character)
            {
                randomTime = RandomTimeBuffs(indicator);

                if (!indicator.StaticBuff)
                {
                    character.Session.SendPacket(
                        $"bf 1 {character.CharacterId} {(character.ChargeValue > 7000 ? 7000 : character.ChargeValue)}.{indicator.Card.CardId}.{(indicator.Card.Duration == 0 ? randomTime : indicator.Card.Duration)} {Level}");
                    character.Session.SendPacket(character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("UNDER_EFFECT"), indicator.Card.Name), 20));
                }
            }

            if (Session is Mate mate)
            {
                randomTime = RandomTimeBuffs(indicator);
                mate.Owner?.Session.SendPacket($"bf 1 {mate.Owner?.CharacterId} 0.{indicator.Card.CardId}.{(indicator.Card.Duration == 0 ? randomTime : indicator.Card.Duration)} {Level}");
            }

            if (!indicator.StaticBuff)
            {
                indicator.RemainingTime = indicator.Card.Duration == 0 ? randomTime : indicator.Card.Duration;
                indicator.Start = DateTime.Now;
            }

            Buffs.Add(indicator);
            if (indicator.Entity != null)
            {
                indicator.Card.BCards.ForEach(c => c.ApplyBCards(Entity, indicator.Entity));
            }
            else
            {
                indicator.Card.BCards.ForEach(c => c.ApplyBCards(Entity));
            }

            if (indicator.Card.EffectId > 0 && indicator.Card.EffectId != 7451)
            {
                Entity.MapInstance?.Broadcast(Entity.GenerateEff(indicator.Card.EffectId));
            }

            if (ObservableBag.TryGetValue(indicator.Card.CardId, out IDisposable value))
            {
                value?.Dispose();
            }

            ObservableBag[indicator.Card.CardId] = Observable
                .Timer(TimeSpan.FromMilliseconds(indicator.RemainingTime * (indicator.StaticBuff ? 1000 : 100)))
                .Subscribe(o =>
                {
                    RemoveBuff(indicator.Card.CardId);

                    if (indicator.Card.TimeoutBuff != 0 &&
                        ServerManager.Instance.RandomNumber() < indicator.Card.TimeoutBuffChance)
                    {
                        AddBuff(new Buff.Buff(indicator.Card.TimeoutBuff, Level));
                    }
                });
        }

        /// <summary>
        /// </summary>
        /// <param name="types"></param>
        /// <param name="level"></param>
        public void DisableBuffs(List<BuffType> types, int level = 100)
        {
            lock (Buffs)
            {
                Buffs.Where(s => types.Contains(s.Card.BuffType) && !s.StaticBuff && s.Card.Level <= level).ToList()
                    .ForEach(s => RemoveBuff(s.Card.CardId));
            }
        }

        public bool HasBuff(BuffType type)
        {
            lock (Buffs)
            {
                return Buffs.Any(s => s.Card.BuffType == type);
            }
        }

        public double GetUpgradeValue(short value)
        {
            switch (Math.Abs(value))
            {
                case 1:
                    return 0.1;

                case 2:
                    return 0.15;

                case 3:
                    return 0.22;

                case 4:
                    return 0.32;

                case 5:
                    return 0.43;

                case 6:
                    return 0.54;

                case 7:
                    return 0.65;

                case 8:
                    return 0.9;

                case 9:
                    return 1.2;

                case 10:
                    return 2;
            }

            return 0;
        }

        public int[] GetBuff(CardType type, byte subtype, bool getStatic = true)
        {
            int value1 = 0;
            int value2 = 0;

            if (getStatic)
            {
                lock (StaticBcards)
                {
                    foreach (BCard entry in StaticBcards.Concat(SkillBcards)
                        .Where(s => s != null && s.Type.Equals((byte)type) && s.SubType.Equals(subtype)))
                    {
                        value1 += entry.IsLevelScaled
                            ? entry.IsLevelDivided ? Level / entry.FirstData : entry.FirstData * Level
                            : entry.FirstData;
                        value2 += entry.SecondData;
                    }
                }
            }

            foreach (Buff.Buff buff in Buffs)
            {
                foreach (BCard entry in buff.Card.BCards.Where(s =>
                    s.Type.Equals((byte)type) && s.SubType.Equals(subtype) &&
                    (s.CastType != 1 ||
                        s.CastType == 1 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now)))
                {
                    value1 += entry.IsLevelScaled
                        ? entry.IsLevelDivided ? buff.Level / entry.FirstData : entry.FirstData * buff.Level
                        : entry.FirstData;
                    value2 += entry.SecondData;
                }
            }

            return new[] { value1, value2 };
        }

        public bool HasBuff(CardType type, byte subtype, bool removeWeaponEffects = false)
        {
            if (removeWeaponEffects)
            {
                return Buffs.Any(buff => buff.Card.BCards.Any(b =>
                    b.Type == (byte)type && b.SubType == subtype &&
                    (b.CastType != 1 ||
                        b.CastType == 1 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now)));
            }

            return Buffs.Any(buff => buff.Card.BCards.Any(b =>
                    b.Type == (byte)type && b.SubType == subtype &&
                    (b.CastType != 1 || b.CastType == 1 &&
                        buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now))) ||
                StaticBcards.Any(s => s.Type.Equals((byte)type) && s.SubType.Equals(subtype));
        }

        public void RemoveBuff(int id, bool removePermaBuff = false)
        {
            Buff.Buff indicator = Buffs.FirstOrDefault(s => s.Card.CardId == id);
            if (indicator == null || !removePermaBuff && indicator.IsPermaBuff && indicator.RemainingTime <= 0)
            {
                return;
            }

            if (indicator.IsPermaBuff && !removePermaBuff)
            {
                AddBuff(indicator);
                return;
            }

            if (ObservableBag.ContainsKey((short)id))
            {
                ObservableBag[(short)id]?.Dispose();
            }
            Buffs.RemoveWhere(s => s.Card.CardId != id, out ConcurrentBag<Buff.Buff> buffs);
            Buffs = buffs;

            if (Session is MapMonster monster)
            {
                monster.ReflectiveBuffs.TryRemove((short)id, out _);
            }

            if (!(Session is Character character))
            {
                return;
            }

            character.ReflectiveBuffs.TryRemove((short)id, out _);

            // Fairy booster
            if (indicator.Card.CardId == 131)
            {
                character.GeneratePairy();
            }

            if (!character.ReflectiveBuffs.Any())
            {
                IsReflecting = false;
            }

            if (indicator.StaticBuff)
            {
                character.Session.SendPacket($"vb {indicator.Card.CardId} 0 {indicator.Card.Duration}");
                character.Session.SendPacket(character.GenerateSay(
                    string.Format(Language.Instance.GetMessageFromKey("EFFECT_TERMINATED"), indicator.Card.Name), 11));
            }
            else
            {
                character.Session.SendPacket($"bf 1 {character.CharacterId} 0.{indicator.Card.CardId}.0 {Level}");
                character.Session.SendPacket(character.GenerateSay(
                    string.Format(Language.Instance.GetMessageFromKey("EFFECT_TERMINATED"), indicator.Card.Name), 20));
            }

            // Fairy Booster
            if (indicator.Card.CardId == 131)
            {
                character.Session.SendPacket(character.GeneratePairy());
            }

            if (indicator.Card.BCards.Any(s => s.Type == (byte)CardType.Move))
            {
                character.LoadSpeed();
                character.LastSpeedChange = DateTime.Now;
                character.Session.SendPacket(character.GenerateCond());
            }

            if (!indicator.Card.BCards.Any(s =>
                    s.Type == (byte)CardType.SpecialActions &&
                    s.SubType.Equals((byte)AdditionalTypes.SpecialActions.Hide)) &&
                indicator.Card.CardId != 559 && indicator.Card.CardId != 560)
            {
                return;
            }

            if (indicator.Card.CardId == 559 && character.TriggerAmbush)
            {
                character.AddBuff(new Buff.Buff(560));
                character.TriggerAmbush = false;
                return;
            }

            character.Invisible = false;
            character.Mates.Where(m => m.IsTeamMember).ToList()
                .ForEach(m => character.MapInstance?.Broadcast(m.GenerateIn()));
            character.MapInstance?.Broadcast(character.GenerateInvisible());
        }

        public void TargetHit(IBattleEntity target, TargetHitType hitType, Skill skill, short? skillEffect = null,
            short? mapX = null, short? mapY = null, ComboDTO skillCombo = null, bool showTargetAnimation = false,
            bool isPvp = false)
        {
            if (target == null || Entity == null)
            {
                return;
            }

            if (!target.IsTargetable(Entity.SessionType(), isPvp) ||
                target.Faction == Entity.Faction && ServerManager.Instance.Act4Maps.Any(m => m == Entity.MapInstance))
            {
                if (Session is Character cha)
                {
                    cha.Session.SendPacket($"cancel 2 {target.GetId()}");
                }

                return;
            }

            MapInstance mapInstance = target.MapInstance;
            int hitmode = 0;
            bool onyxWings = false;
            int damage = DamageHelper.Instance.GenerateDamage(this, target, skill, ref hitmode, ref onyxWings);

            if (skill != null && SkillHelper.Instance.NoDamageSkills != null)
            {
                if (SkillHelper.Instance.NoDamageSkills.Any(s => s == skill.SkillVNum))
                {
                    target.DealtDamage = 0;
                    damage = 0;
                }
            }

            if (Session is Character charact && mapInstance != null && hitmode != 1)
            {
                if (onyxWings)
                {
                    short onyxX = (short)(charact.PositionX + 2);
                    short onyxY = (short)(charact.PositionY + 2);
                    int onyxId = mapInstance.GetNextId();
                    var onyx = new MapMonster
                    {
                        MonsterVNum = 2371,
                        MapX = onyxX,
                        MapY = onyxY,
                        MapMonsterId = onyxId,
                        IsHostile = false,
                        IsMoving = false,
                        ShouldRespawn = false
                    };
                    mapInstance.Broadcast($"guri 31 1 {charact.CharacterId} {onyxX} {onyxY}");
                    onyx.Initialize(mapInstance);
                    mapInstance.AddMonster(onyx);
                    mapInstance.Broadcast(onyx.GenerateIn());
                    target.GetDamage(target.DealtDamage / 2, Entity, false);
                    Observable.Timer(TimeSpan.FromMilliseconds(350)).Subscribe(o =>
                    {
                        mapInstance.Broadcast(
                            $"su 3 {onyxId} {(target is Character ? "1" : "3")} {target.GetId()} -1 0 -1 {skill.Effect} -1 -1 1 {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {(target.BattleEntity.IsReflecting ? 0 : target.DealtDamage) / 2} 0 0");
                        mapInstance.RemoveMonster(onyx);
                        mapInstance.Broadcast(onyx.GenerateOut());
                    });
                }

                if (target is Character tchar)
                {
                    if (tchar.ReflectiveBuffs.Any())
                    {
                        int? multiplier = 0;

                        foreach (KeyValuePair<short, int?> entry in tchar.ReflectiveBuffs)
                        {
                            multiplier += entry.Value;
                        }
                        ushort damaged = (ushort)(damage > tchar.Level * multiplier ? tchar.Level * multiplier : damage);
                        mapInstance.Broadcast(
                            $"su 1 {tchar.GetId()} 1 {charact.GetId()} -1 0 -1 {skill.Effect} -1 -1 1 {(int)(tchar.Hp / (double)target.MaxHp * 100)} {damaged} 0 1");
                        charact.Hp = charact.Hp - damaged <= 0 ? 1 : charact.Hp - damaged;
                        charact.Session.SendPacket(charact.GenerateStat());
                        target.DealtDamage = 0;
                    }
                }
                else if (target is MapMonster tmon)
                {
                    if (tmon.ReflectiveBuffs.Any())
                    {
                        int? multiplier = 0;

                        foreach (KeyValuePair<short, int?> entry in tmon.ReflectiveBuffs)
                        {
                            multiplier += entry.Value;
                        }

                        ushort damaged = (ushort)(damage > tmon.Monster.Level * multiplier ? tmon.Monster.Level * multiplier : damage);
                        charact.Hp -= charact.Hp - damaged <= 0 ? 1 : charact.Hp - damaged;
                        charact.Session.SendPacket(charact.GenerateStat());
                        mapInstance.Broadcast(
                            $"su 3 {tmon.GetId()} 1 {charact.GetId()} -1 0 -1 {skill.Effect} -1 -1 1 {(int)(tmon.CurrentHp / (double)target.MaxHp * 100)} {damaged} 0 1");
                        target.DealtDamage = 0;
                    }
                }
            }

            if (target.GetSession() is Character character)
            {
                damage = (ushort)(character.HasGodMode ? 0 : damage);
                target.DealtDamage = (ushort)(character.HasGodMode ? 0 : damage);
                if (character.IsSitting)
                {
                    character.IsSitting = false;
                    character.MapInstance.Broadcast(character.GenerateRest());
                }
            }
            else if (target.GetSession() is Mate mate)
            {
                if (mate.IsSitting)
                {
                    mate.IsSitting = false;
                    mate.Owner.MapInstance.Broadcast(mate.GenerateRest());
                }
            }

            int castTime = 0;
            if (skill != null && skill.CastEffect != 0)
            {
                Entity.MapInstance.Broadcast(Entity.GenerateEff(skill.CastEffect), Entity.GetPos().X,
                    Entity.GetPos().Y);
                castTime = skill.CastTime * 100;
            }

            Observable.Timer(TimeSpan.FromMilliseconds(castTime)).Subscribe(o => TargetHit2(target, hitType, skill,
                damage, hitmode, skillEffect, mapX, mapY, skillCombo, showTargetAnimation, isPvp));
        }

        private void TargetHit2(IBattleEntity target, TargetHitType hitType, Skill skill, int damage, int hitmode,
            short? skillEffect = null, short? mapX = null, short? mapY = null, ComboDTO skillCombo = null,
            bool showTargetAnimation = false, bool isPvp = false, bool isRange = false)
        {
            target.GetDamage(target.BattleEntity.IsReflecting ? 0 : target.DealtDamage, Entity, !(Session is MapMonster mon && mon.IsInvicible));
            string str =
                $"su {(byte)Entity.SessionType()} {Entity.GetId()} {(byte)target.SessionType()} {target.GetId()} {skill?.SkillVNum ?? 0} {skill?.Cooldown ?? 0}";
            switch (hitType)
            {
                case TargetHitType.SingleTargetHit:
                    str +=
                        $" {skill?.AttackAnimation ?? 11} {skill?.Effect ?? skillEffect ?? 0} {Entity.GetPos().X} {Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {(target.BattleEntity.IsReflecting ? 0 : target.DealtDamage)} {hitmode} {skill?.SkillType - 1 ?? 0}";
                    break;

                case TargetHitType.SingleTargetHitCombo:
                    str +=
                        $" {skillCombo?.Animation ?? 0} {skillCombo?.Effect ?? 0} {Entity.GetPos().X} {Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {(target.BattleEntity.IsReflecting ? 0 : target.DealtDamage)} {hitmode} {skill.SkillType - 1}";
                    break;

                case TargetHitType.SingleAOETargetHit:
                    switch (hitmode)
                    {
                        case 1:
                            hitmode = 4;
                            break;

                        case 3:
                            hitmode = 6;
                            break;

                        default:
                            hitmode = 5;
                            break;
                    }

                    if (showTargetAnimation)
                    {
                        Entity.MapInstance.Broadcast(
                            $" {skill?.AttackAnimation ?? 0} {skill?.Effect ?? 0} 0 0 {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} 0 0 {skill.SkillType - 1}");
                    }

                    str +=
                        $" {skill?.AttackAnimation ?? 0} {skill?.Effect ?? 0} {Entity.GetPos().X} {Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {(target.BattleEntity.IsReflecting ? 0 : target.DealtDamage)} {hitmode} {skill.SkillType - 1}";
                    break;

                case TargetHitType.AOETargetHit:
                    switch (hitmode)
                    {
                        case 1:
                            hitmode = 4;
                            break;

                        case 3:
                            hitmode = 6;
                            break;

                        default:
                            hitmode = 5;
                            break;
                    }

                    str +=
                        $" {skill?.AttackAnimation ?? 0} {skill?.Effect ?? 0} {Entity.GetPos().X} {Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {(target.BattleEntity.IsReflecting ? 0 : target.DealtDamage)} {hitmode} {skill.SkillType - 1}";
                    break;

                case TargetHitType.ZoneHit:
                    str +=
                        $" {skill?.AttackAnimation ?? 0} {skillEffect ?? 0} {mapX ?? Entity.GetPos().X} {mapY ?? Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {(target.BattleEntity.IsReflecting ? 0 : target.DealtDamage)} 5 {skill.SkillType - 1}";
                    break;

                case TargetHitType.SpecialZoneHit:
                    str +=
                        $" {skill?.AttackAnimation ?? 0} {skillEffect ?? 0} {Entity.GetPos().X} {Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {(target.BattleEntity.IsReflecting ? 0 : target.DealtDamage)} 0 {skill.SkillType - 1}";
                    break;
            }

            Entity.MapInstance.Broadcast(str);

            bool isBoss = false;

            if (Entity.GetSession() is Character character)
            {
                character.LastSkillUse = DateTime.Now;
                RemoveBuff(85); // Hideout
            }
            else if (Entity.GetSession() is Mate mate)
            {
                mate.LastSkillUse = DateTime.Now;
            }

            if (target.GetSession() is MapMonster monster)
            {
                if (monster.Target == null)
                {
                    monster.LastSkill = DateTime.Now;
                }

                monster.Target = Entity;
                isBoss = monster.IsBoss;
                if (isBoss)
                {
                    Entity.MapInstance?.Broadcast(monster.GenerateBoss());
                }

                monster.DamageList.AddOrUpdate(Entity, damage, (key, oldValue) => oldValue + damage);
            }

            if (!isBoss && skill != null && hitmode != 1)
            {
                if ((target.BattleEntity.CostumeHatBcards == null || !target.BattleEntity.CostumeHatBcards.Any()) && target is Character targetCharacter)
                {
                    var hat = targetCharacter.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.CostumeHat, InventoryType.Wear);
                    hat?.Item.BCards.ForEach(s => target.BattleEntity.CostumeHatBcards.Add(s));
                }
                if ((target.BattleEntity.CostumeSuitBcards == null || !target.BattleEntity.CostumeSuitBcards.Any()) && target is Character targetCharacter2)
                {
                    var costume = targetCharacter2.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.CostumeHat, InventoryType.Wear);
                    costume?.Item.BCards.ForEach(s => target.BattleEntity.CostumeSuitBcards.Add(s));
                }
                foreach (BCard bcard in target.BattleEntity.CostumeSuitBcards.Where(s => s != null))
                {
                    switch ((CardType)bcard.Type)
                    {
                        case CardType.Buff:
                            var b = new Buff.Buff(bcard.SecondData);
                            switch (b.Card?.BuffType)
                            {
                                case BuffType.Bad:
                                    bcard.ApplyBCards(Entity, Entity);
                                    break;

                                case BuffType.Good:
                                case BuffType.Neutral:
                                    bcard.ApplyBCards(target, Entity);
                                    break;
                            }

                            break;
                    }
                }
                foreach (BCard bcard in target.BattleEntity.CostumeSuitBcards.Where(s => s != null))
                {
                    switch ((CardType)bcard.Type)
                    {
                        case CardType.Buff:
                            var b = new Buff.Buff(bcard.SecondData);
                            switch (b.Card?.BuffType)
                            {
                                case BuffType.Bad:
                                    bcard.ApplyBCards(Entity, Entity);
                                    break;

                                case BuffType.Good:
                                case BuffType.Neutral:
                                    bcard.ApplyBCards(target, Entity);
                                    break;
                            }

                            break;
                    }
                }
                foreach (BCard bc in StaticBcards.Where(s => s != null))
                {
                    switch ((CardType)bc.Type)
                    {
                        case CardType.Buff:
                            var b = new Buff.Buff(bc.SecondData);
                            switch (b.Card?.BuffType)
                            {
                                case BuffType.Bad:
                                    bc.ApplyBCards(target, Entity);
                                    break;
                                case BuffType.Good:
                                case BuffType.Neutral:
                                    bc.ApplyBCards(Entity, Entity);
                                    break;
                            }
                            break;
                    }
                }
                foreach (BCard bcard in skill.BCards.Where(b => b != null))
                {
                    switch ((CardType)bcard.Type)
                    {
                        case CardType.Buff:
                            var b = new Buff.Buff(bcard.SecondData);
                            switch (b.Card?.BuffType)
                            {
                                case BuffType.Bad:
                                    bcard.ApplyBCards(target, Entity);
                                    break;

                                case BuffType.Good:
                                case BuffType.Neutral:
                                    bcard.ApplyBCards(Entity, Entity);
                                    break;
                            }

                            break;

                        case CardType.HealingBurningAndCasting:
                            switch ((AdditionalTypes.HealingBurningAndCasting)bcard.SubType)
                            {
                                case AdditionalTypes.HealingBurningAndCasting.RestoreHP:
                                case AdditionalTypes.HealingBurningAndCasting.RestoreHPWhenCasting:
                                    bcard.ApplyBCards(Entity, Entity);
                                    break;

                                default:
                                    bcard.ApplyBCards(target, Entity);
                                    break;
                            }

                            break;

                        case CardType.MeditationSkill:
                            bcard.ApplyBCards(Entity);
                            break;

                        default:
                            bcard.ApplyBCards(target, Entity);
                            break;
                    }
                }
            }

            if (skill == null || skill.Range <= 0 && skill.TargetRange <= 0 || isRange ||
                !(Entity.GetSession() is MapMonster))
            {
                return;
            }

            foreach (IBattleEntity entitiesInRange in Entity.MapInstance
                ?.GetBattleEntitiesInRange(Entity.GetPos(), skill.TargetRange)
                .Where(e => e != target && e.IsTargetable(Entity.SessionType())))
            {
                TargetHit2(entitiesInRange, TargetHitType.SingleTargetHit, skill, damage, hitmode, isRange: true);
            }
        }

        #endregion
    }
}