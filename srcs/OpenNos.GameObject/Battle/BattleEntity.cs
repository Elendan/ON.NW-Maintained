using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Data;
using OpenNos.GameObject.Battle.Args;
using OpenNos.GameObject.Buff;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Event.CALIGOR;
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
            SkillBcards = new List<BCard>();
            OnDeathEvents = new ConcurrentBag<EventContainer>();
            OnHitEvents = new ConcurrentBag<EventContainer>();
            ObservableBag = new ConcurrentDictionary<short, IDisposable>();
            ShellOptionArmor = new ConcurrentBag<EquipmentOptionDTO>();
            ShellOptionsMain = new ConcurrentBag<EquipmentOptionDTO>();
            ShellOptionsSecondary = new ConcurrentBag<EquipmentOptionDTO>();

            if (Session is Character character)
            {
                Level = character.Level;
                Morale = character.Level;
                HpMax = character.MaxHp;
                MpMax = character.MaxMp;
                EntityType = EntityType.Player;
                MinDamage = character.MinHit;
                MaxDamage = character.MaxHit;
                HitRate = character.HitRate;
                CriticalChance = character.HitCriticalRate;
                CriticalRate = character.HitCritical;
                Morale = character.Level;
                FireResistance = character.FireResistance;
                WaterResistance = character.WaterResistance;
                LightResistance = character.LightResistance;
                DarkResistance = character.DarkResistance;
                PositionX = character.PositionX;
                PositionY = character.PositionY;
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
            Morale = npcMonster.Level;
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

        public ConcurrentBag<Buff.Buff> Buffs { get; set; }

        public ConcurrentBag<BCard> StaticBcards { get; set; }

        public int HpMax { get; set; }

        public int Morale { get; set; }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public int MpMax { get; set; }

        public int Resistance { get; set; }

        public EntityType EntityType { get; set; }

        public List<BCard> SkillBcards { get; set; }

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

        public byte DefenceUpgrade { get; set; }

        public int DefenceRate { get; set; }

        public int DistanceDefenceRate { get; set; }

        public int CloseDefence { get; set; }

        public int RangedDefence { get; set; }

        public int MagicDefence { get; set; }

        #endregion

        #endregion

        #region Methods

        public void DefineAttackType(Skill skill)
        {
            WearableInstance weapon = null;

            if (Session is Character character)
            {
                if (skill != null)
                {
                    switch (skill.Type)
                    {
                        case 0:
                            AttackType = AttackType.Close;
                            if (character.Class == ClassType.Archer)
                            {
                                MinDamage = character.MinDistance;
                                MaxDamage = character.MaxDistance;
                                HitRate = character.DistanceRate;
                                CriticalChance = character.DistanceCriticalRate;
                                CriticalRate = character.DistanceCritical;
                                weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                            }
                            else
                            {
                                weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                            }
                            break;

                        case 1:
                            AttackType = AttackType.Ranged;
                            if (character.Class == ClassType.Adventurer || character.Class == ClassType.Swordman || character.Class == ClassType.Magician)
                            {
                                MinDamage = character.MinDistance;
                                MaxDamage = character.MaxDistance;
                                HitRate = character.DistanceRate;
                                CriticalChance = character.DistanceCriticalRate;
                                CriticalRate = character.DistanceCritical;
                                weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                            }
                            else
                            {
                                weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                            }
                            break;

                        case 2:
                            AttackType = AttackType.Magical;
                            weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                            break;

                        case 3:
                            weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                            switch (character.Class)
                            {
                                case ClassType.Adventurer:
                                case ClassType.Swordman:
                                    AttackType = AttackType.Close;
                                    break;

                                case ClassType.Archer:
                                    AttackType = AttackType.Ranged;
                                    break;

                                case ClassType.Magician:
                                    AttackType = AttackType.Magical;
                                    break;
                            }
                            break;

                        case 5:
                            AttackType = AttackType.Close;
                            switch (character.Class)
                            {
                                case ClassType.Adventurer:
                                case ClassType.Swordman:
                                case ClassType.Magician:
                                    weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                                    break;

                                case ClassType.Archer:
                                    weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                    switch (character.Class)
                    {
                        case ClassType.Adventurer:
                        case ClassType.Swordman:
                            AttackType = AttackType.Close;
                            break;

                        case ClassType.Archer:
                            AttackType = AttackType.Ranged;
                            break;

                        case ClassType.Magician:
                            AttackType = AttackType.Magical;
                            break;
                    }
                }

                if (weapon != null)
                {
                    AttackUpgrade = weapon.Upgrade;
                    WeaponDamageMinimum = weapon.DamageMinimum + weapon.Item.DamageMinimum;
                    WeaponDamageMaximum = weapon.DamageMaximum + weapon.Item.DamageMinimum;
                }

                var armor = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Armor, InventoryType.Wear);
                if (armor != null)
                {
                    DefenseUpgrade = armor.Upgrade;
                    ArmorMeleeDefense = armor.CloseDefence + armor.Item.CloseDefence;
                    ArmorRangeDefense = armor.DistanceDefence + armor.Item.DistanceDefence;
                    ArmorMagicalDefense = armor.MagicDefence + armor.Item.MagicDefence;
                }

                //Todo: Get cellon options !
                //CellonOptions = Session.Character.CellonOptions.GetAllItems();

                MeleeDefense = character.Defence - ArmorMeleeDefense;
                MeleeDefenseDodge = character.DefenceRate;
                RangeDefense = character.DistanceDefence - ArmorRangeDefense;
                RangeDefenseDodge = character.DistanceDefenceRate;
                MagicalDefense = character.MagicalDefence - ArmorMagicalDefense;
                Element = character.Element;
                ElementRate = character.ElementRate + character.ElementRateSp;
            }
            else if (Session is Mate mate)
            {
                HpMax = mate.MaxHp;
                MpMax = mate.MaxMp;

                var mateWeapon = (WearableInstance)mate.WeaponInstance;
                var mateArmor = (WearableInstance)mate.ArmorInstance;
                var mateGloves = (WearableInstance)mate.GlovesInstance;
                var mateBoots = (WearableInstance)mate.BootsInstance;

                Buffs = mate.Buffs;
                //BCards = mate.Monster.BCards.ToList();
                Level = mate.Level;
                EntityType = EntityType.Mate;
                MinDamage = (mateWeapon?.DamageMinimum ?? 0) /*+ mate.BaseDamage*/ + mate.Monster.DamageMinimum;
                MaxDamage = (mateWeapon?.DamageMaximum ?? 0) /*+ mate.BaseDamage */ + mate.Monster.DamageMaximum;
                WeaponDamageMinimum = (mateWeapon?.DamageMinimum ?? MinDamage);
                WeaponDamageMaximum = (mateWeapon?.DamageMaximum ?? MaxDamage);
                PositionX = mate.PositionX;
                PositionY = mate.PositionY;
                HitRate = mate.Monster.Concentrate + (mateWeapon?.HitRate ?? 0);
                CriticalChance = mate.Monster.CriticalChance + (mateWeapon?.CriticalLuckRate ?? 0);
                CriticalRate = mate.Monster.CriticalRate + (mateWeapon?.CriticalRate ?? 0);
                Morale = mate.Level;
                AttackUpgrade = mateWeapon?.Upgrade ?? mate.Attack;
                FireResistance = mate.Monster.FireResistance + (mateGloves?.FireResistance ?? 0) + (mateBoots?.FireResistance ?? 0);
                WaterResistance = mate.Monster.WaterResistance + (mateGloves?.FireResistance ?? 0) + (mateBoots?.FireResistance ?? 0);
                LightResistance = mate.Monster.LightResistance + (mateGloves?.FireResistance ?? 0) + (mateBoots?.FireResistance ?? 0);
                DarkResistance = mate.Monster.DarkResistance + (mateGloves?.FireResistance ?? 0) + (mateBoots?.FireResistance ?? 0);
                AttackType = (AttackType)mate.Monster.AttackClass;

                DefenseUpgrade = mateArmor?.Upgrade ?? mate.Defence;
                MeleeDefense = (mateArmor?.CloseDefence ?? 0)/* + mate.MeleeDefense */ + mate.Monster.CloseDefence;
                RangeDefense = (mateArmor?.DistanceDefence ?? 0) /*+ mate.RangeDefense */ + mate.Monster.DistanceDefence;
                MagicalDefense = (mateArmor?.MagicDefence ?? 0) /*+ mate.MagicalDefense */ + mate.Monster.MagicDefence;
                MeleeDefenseDodge = (mateArmor?.DefenceDodge ?? 0) /*+ mate.MeleeDefenseDodge */ + mate.Monster.DefenceDodge;
                RangeDefenseDodge = (mateArmor?.DistanceDefenceDodge ?? 0) /*+ mate.RangeDefenseDodge */ + mate.Monster.DistanceDefenceDodge;

                ArmorMeleeDefense = mateArmor?.CloseDefence ?? MeleeDefense;
                ArmorRangeDefense = mateArmor?.DistanceDefence ?? RangeDefense;
                ArmorMagicalDefense = mateArmor?.MagicDefence ?? MagicalDefense;

                Element = mate.Monster.Element;
                ElementRate = mate.Monster.ElementRate;
            }
            else if (Session is MapMonster monster)
            {
                HpMax = monster.Monster.MaxHP;
                MpMax = monster.Monster.MaxMP;
                Buffs = monster.Buffs;
                //BCards = monster.Monster.BCards.ToList();
                Level = monster.Monster.Level;
                EntityType = EntityType.Monster;
                MinDamage = 0;
                MaxDamage = 0;
                WeaponDamageMinimum = monster.Monster.DamageMinimum;
                WeaponDamageMaximum = monster.Monster.DamageMaximum;
                HitRate = monster.Monster.Concentrate;
                CriticalChance = monster.Monster.CriticalChance;
                CriticalRate = monster.Monster.CriticalRate;
                Morale = monster.Monster.Level;
                AttackUpgrade = monster.Monster.AttackUpgrade;
                FireResistance = monster.Monster.FireResistance;
                WaterResistance = monster.Monster.WaterResistance;
                LightResistance = monster.Monster.LightResistance;
                DarkResistance = monster.Monster.DarkResistance;
                PositionX = monster.MapX;
                PositionY = monster.MapY;
                AttackType = (AttackType)monster.Monster.AttackClass;
                DefenseUpgrade = monster.Monster.DefenceUpgrade;
                MeleeDefense = monster.Monster.CloseDefence;
                MeleeDefenseDodge = monster.Monster.DefenceDodge;
                RangeDefense = monster.Monster.DistanceDefence;
                RangeDefenseDodge = monster.Monster.DistanceDefenceDodge;
                MagicalDefense = monster.Monster.MagicDefence;
                ArmorMeleeDefense = monster.Monster.CloseDefence;
                ArmorRangeDefense = monster.Monster.DistanceDefence;
                ArmorMagicalDefense = monster.Monster.MagicDefence;
                Element = monster.Monster.Element;
                ElementRate = monster.Monster.ElementRate;
            }
            else if (Session is MapNpc npc)
            {
                HpMax = npc.Npc.MaxHP;
                MpMax = npc.Npc.MaxMP;

                //npc.Buff.CopyTo(Buffs);
                //BCards = npc.Npc.BCards.ToList();
                Level = npc.Npc.Level;
                EntityType = EntityType.Monster;
                MinDamage = 0;
                MaxDamage = 0;
                WeaponDamageMinimum = npc.Npc.DamageMinimum;
                WeaponDamageMaximum = npc.Npc.DamageMaximum;
                HitRate = npc.Npc.Concentrate;
                CriticalChance = npc.Npc.CriticalChance;
                CriticalRate = npc.Npc.CriticalRate;
                Morale = npc.Npc.Level;
                AttackUpgrade = npc.Npc.AttackUpgrade;
                FireResistance = npc.Npc.FireResistance;
                WaterResistance = npc.Npc.WaterResistance;
                LightResistance = npc.Npc.LightResistance;
                DarkResistance = npc.Npc.DarkResistance;
                PositionX = npc.MapX;
                PositionY = npc.MapY;
                AttackType = (AttackType)npc.Npc.AttackClass;
                DefenseUpgrade = npc.Npc.DefenceUpgrade;
                MeleeDefense = npc.Npc.CloseDefence;
                MeleeDefenseDodge = npc.Npc.DefenceDodge;
                RangeDefense = npc.Npc.DistanceDefence;
                RangeDefenseDodge = npc.Npc.DistanceDefenceDodge;
                MagicalDefense = npc.Npc.MagicDefence;
                ArmorMeleeDefense = npc.Npc.CloseDefence;
                ArmorRangeDefense = npc.Npc.DistanceDefence;
                ArmorMagicalDefense = npc.Npc.MagicDefence;
                Element = npc.Npc.Element;
                ElementRate = npc.Npc.ElementRate;
            }

        }

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
                        break;
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

        private static int[] GetBuff(byte level, IReadOnlyCollection<Buff.Buff> buffs, IReadOnlyCollection<BCard> bcards, CardType type,
            byte subtype, BuffType btype, ref int count)
        {
            int value1 = 0;
            int value2 = 0;
            int value3 = 0;

            IEnumerable<BCard> cards;

            if (bcards != null && btype.Equals(BuffType.Good))
            {
                cards = subtype % 10 == 1
                    ? bcards.Where(s =>
                        s.Type.Equals((byte)type) && s.SubType.Equals((byte)(subtype / 10)) && s.FirstData >= 0)
                    : bcards.Where(s =>
                        s.Type.Equals((byte)type) && s.SubType.Equals((byte)(subtype / 10))
                        && (s.FirstData <= 0 || s.ThirdData < 0));

                foreach (BCard entry in cards)
                {
                    if (entry.IsLevelScaled)
                    {
                        if (entry.IsLevelDivided)
                        {
                            value1 += level / entry.FirstData;
                        }
                        else
                        {
                            value1 += entry.FirstData * level;
                        }
                    }
                    else
                    {
                        value1 += entry.FirstData;
                    }

                    value2 += entry.SecondData;
                    value3 += entry.ThirdData;
                    count++;
                }
            }

            if (buffs != null)
            {
                foreach (Buff.Buff buff in buffs.Where(b => b.Card.BuffType.Equals(btype)))
                {
                    cards = subtype % 10 == 1
                        ? buff.Card.BCards.Where(s =>
                            s.Type.Equals((byte)type) && s.SubType.Equals((byte)(subtype / 10))
                            && (s.CastType != 1 || (s.CastType == 1
                                                 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now))
                            && s.FirstData >= 0)
                        : buff.Card.BCards.Where(s =>
                            s.Type.Equals((byte)type) && s.SubType.Equals((byte)(subtype / 10))
                            && (s.CastType != 1 || (s.CastType == 1
                                                 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now))
                            && s.FirstData <= 0);

                    foreach (BCard entry in cards)
                    {
                        if (entry.IsLevelScaled)
                        {
                            if (entry.IsLevelDivided)
                            {
                                value1 += buff.Level / entry.FirstData;
                            }
                            else
                            {
                                value1 += entry.FirstData * buff.Level;
                            }
                        }
                        else
                        {
                            value1 += entry.FirstData;
                        }

                        value2 += entry.SecondData;
                        value3 += entry.ThirdData;
                        count++;
                    }
                }
            }

            return new[] { value1, value2, value3 };
        }

        public int GenerateDamage(IBattleEntity targetEntity, Skill skill, ref int hitmode, ref bool onyxEffect)
        {
            BattleEntity target = targetEntity?.BattleEntity;
            DefineAttackType(skill);
            if (target == null)
            {
                return 0;
            }

            int[] GetAttackerBenefitingBuffs(BCardType.CardType type, byte subtype)
            {
                int value1 = 0;
                int value2 = 0;
                int value3 = 0;
                int temp = 0;

                int[] tmp = GetBuff(Level, Buffs, StaticBcards, type, subtype, BuffType.Good,
                    ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];
                tmp = GetBuff(Level, Buffs, StaticBcards, type, subtype, BuffType.Neutral,
                    ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];
                tmp = GetBuff(target.Level, target.Buffs, target.StaticBcards, type, subtype, BuffType.Bad, ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];

                return new[] { value1, value2, value3, temp };
            }

            int[] GetDefenderBenefitingBuffs(BCardType.CardType type, byte subtype)
            {
                int value1 = 0;
                int value2 = 0;
                int value3 = 0;
                int temp = 0;

                int[] tmp = GetBuff(target.Level, target.Buffs, target.StaticBcards, type, subtype, BuffType.Good,
                    ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];
                tmp = GetBuff(target.Level, target.Buffs, target.StaticBcards, type, subtype, BuffType.Neutral,
                    ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];
                tmp = GetBuff(Level, Buffs, StaticBcards, type, subtype, BuffType.Bad, ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];

                return new[] { value1, value2, value3, temp };
            }

            int GetShellWeaponEffectValue(ShellOptionType effectType)
            {
                return ShellOptionsMain.FirstOrDefault(s => s.Type == (byte)effectType)?.Value ?? 0;
            }

            int GetShellArmorEffectValue(ShellOptionType effectType)
            {
                return ShellOptionArmor.FirstOrDefault(s => s.Type == (byte)effectType)?.Value ?? 0;
            }

            if (skill != null)
            {
                //Todo: Clean this afterwards
                SkillBcards.AddRange(skill.BCards);
            }

            #region Basic Buff Initialisation

            Morale +=
                GetAttackerBenefitingBuffs(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased)[0];
            Morale +=
                GetDefenderBenefitingBuffs(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased)[0];
            target.Morale +=
                GetDefenderBenefitingBuffs(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased)[0];
            target.Morale +=
                GetAttackerBenefitingBuffs(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased)[0];

            AttackUpgrade += (byte)GetAttackerBenefitingBuffs(CardType.AttackPower,
                (byte)AdditionalTypes.AttackPower.AttackLevelIncreased)[0];
            AttackUpgrade += (byte)GetDefenderBenefitingBuffs(CardType.AttackPower,
                (byte)AdditionalTypes.AttackPower.AttackLevelDecreased)[0];
            target.DefenceUpgrade += (byte)GetDefenderBenefitingBuffs(CardType.Defence,
                (byte)AdditionalTypes.Defence.DefenceLevelIncreased)[0];
            target.DefenceUpgrade += (byte)GetAttackerBenefitingBuffs(CardType.Defence,
                (byte)AdditionalTypes.Defence.DefenceLevelDecreased)[0];

            int[] attackerpercentdamage = GetAttackerBenefitingBuffs(CardType.RecoveryAndDamagePercent, 11);
            int[] defenderpercentdefense = GetDefenderBenefitingBuffs(CardType.RecoveryAndDamagePercent, 2);

            //int[] attackerpercentdamage2 = GetDefenderBenefitingBuffs(CardType.RecoveryAndDamagePercent, 11);
            //int[] defenderpercentdefense2 = GetAttackerBenefitingBuffs(CardType.RecoveryAndDamagePercent, 2);

            if (attackerpercentdamage[3] != 0)
            {
                return (ushort)(target.HpMax / 100 * attackerpercentdamage[2]);
            }

            if (defenderpercentdefense[3] != 0)
            {
                return (ushort)(target.HpMax / 100 * Math.Abs(defenderpercentdefense[2]));
            }

            /*
             *
             * Percentage Boost categories:
             *  1.: Adds to Total Damage
             *  2.: Adds to Normal Damage
             *  3.: Adds to Base Damage
             *  4.: Adds to Defense
             *  5.: Adds to Element
             *
             * Buff Effects get added, whereas
             * Shell Effects get multiplied afterwards.
             *
             * Simplified Example on Defense (Same for Attack):
             *  - 1k Defense
             *  - Costume(+5% Defense)
             *  - Defense Potion(+20% Defense)
             *  - S-Defense Shell with 20% Boost
             *
             * Calculation:
             *  1000 * 1.25 * 1.2 = 1500
             *  Def    Buff   Shell Total
             *
             * Keep in Mind that after each step, one has
             * to round the current value down if necessary
             *
             * Static Boost categories:
             *  1.: Adds to Total Damage
             *  2.: Adds to Normal Damage
             *  3.: Adds to Base Damage
             *  4.: Adds to Defense
             *  5.: Adds to Element
             *
             */

            #region Definitions

            double boostCategory1 = 1;
            double boostCategory2 = 1;
            double boostCategory3 = 1;
            double boostCategory4 = 1;
            double boostCategory5 = 1;
            double shellBoostCategory1 = 1;
            double shellBoostCategory2 = 1;
            double shellBoostCategory3 = 1;
            double shellBoostCategory4 = 1;
            double shellBoostCategory5 = 1;
            int staticBoostCategory1 = 0;
            int staticBoostCategory2 = 0;
            int staticBoostCategory3 = 0;
            int staticBoostCategory4 = 0;
            int staticBoostCategory5 = 0;

            #endregion

            #region Type 1

            #region Static

            // None for now

            #endregion

            #region Boost

            boostCategory1 +=
                GetAttackerBenefitingBuffs(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased)
                    [0] / 100D;
            boostCategory1 +=
                GetDefenderBenefitingBuffs(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageDecreased)
                    [0] / 100D;
            boostCategory1 +=
                GetAttackerBenefitingBuffs(BCardType.CardType.Item, (byte)AdditionalTypes.Item.AttackIncreased)[0]
                / 100D;
            boostCategory1 +=
                GetDefenderBenefitingBuffs(BCardType.CardType.Item, (byte)AdditionalTypes.Item.DefenceIncreased)[0]
                / 100D;
            shellBoostCategory1 += GetShellWeaponEffectValue(ShellOptionType.SDamagePercentage) / 100D;

            //Todo: Review condition
            /*if ((EntityType == EntityType.Player || EntityType == EntityType.Mate)
                && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
            {*/
                boostCategory1 += GetAttackerBenefitingBuffs(BCardType.CardType.SpecialisationBuffResistance,
                                      (byte)AdditionalTypes.SpecialisationBuffResistance.IncreaseDamageInPVP)[0]
                                  / 100D;
                boostCategory1 += GetAttackerBenefitingBuffs(BCardType.CardType.LeonaPassiveSkill,
                                      (byte)AdditionalTypes.LeonaPassiveSkill.AttackIncreasedInPVP)[0] / 100D;
                shellBoostCategory1 += GetShellWeaponEffectValue(ShellOptionType.PvpDamagePercentage) / 100D;
            //}

            #endregion

            #endregion

            #region Type 2

            #region Static

            // None for now

            #endregion

            #region Boost

            boostCategory2 +=
                GetDefenderBenefitingBuffs(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageDecreased)
                    [0] / 100D;

            //TODO: Review condition
            /* ((EntityType == EntityType.Player || EntityType == EntityType.Mate)
                && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
            {*/
                boostCategory2 += GetDefenderBenefitingBuffs(BCardType.CardType.SpecialisationBuffResistance,
                                      (byte)AdditionalTypes.SpecialisationBuffResistance.DecreaseDamageInPVP)[0]
                                  / 100D;
                boostCategory2 += GetDefenderBenefitingBuffs(BCardType.CardType.LeonaPassiveSkill,
                                      (byte)AdditionalTypes.LeonaPassiveSkill.AttackDecreasedInPVP)[0] / 100D;
           //}

            #endregion

            #endregion

            #region Type 3

            #region Static

            staticBoostCategory3 += GetAttackerBenefitingBuffs(BCardType.CardType.AttackPower,
                (byte)AdditionalTypes.AttackPower.AllAttacksIncreased)[0];
            staticBoostCategory3 += GetDefenderBenefitingBuffs(BCardType.CardType.AttackPower,
                (byte)AdditionalTypes.AttackPower.AllAttacksDecreased)[0];
            staticBoostCategory3 += GetShellWeaponEffectValue(ShellOptionType.IncreaseDamage);

            #endregion

            #region Soft-Damage

            int[] soft = GetAttackerBenefitingBuffs(BCardType.CardType.IncreaseDamage,
                (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability);
            int[] skin = GetAttackerBenefitingBuffs(BCardType.CardType.EffectSummon,
                (byte)AdditionalTypes.EffectSummon.DamageBoostOnHigherLvl);
            if (Level < target.Level)
            {
                soft[0] += skin[0];
                soft[1] += skin[1];
            }

            if (ServerManager.Instance.RandomNumber() < soft[0])
            {
                boostCategory3 += soft[1] / 100D;
                //Todo: Add an EntityType to the BattleEntity
                if (Session is Character c)
                {
                    c.Session?.CurrentMapInstance?.Broadcast(c.GenerateEff(15));
                }
            }

            #endregion

            #endregion

            #region Type 4

            #region Static

            staticBoostCategory4 +=
                GetDefenderBenefitingBuffs(BCardType.CardType.Defence, (byte)AdditionalTypes.Defence.AllIncreased)[0];
            staticBoostCategory4 +=
                GetAttackerBenefitingBuffs(BCardType.CardType.Defence, (byte)AdditionalTypes.Defence.AllDecreased)[0];

            #endregion

            #region Boost

            boostCategory4 += GetDefenderBenefitingBuffs(BCardType.CardType.DodgeAndDefencePercent,
                                  (byte)AdditionalTypes.DodgeAndDefencePercent.DefenceIncreased)[0] / 100D;
            boostCategory4 += GetAttackerBenefitingBuffs(BCardType.CardType.DodgeAndDefencePercent,
                                  (byte)AdditionalTypes.DodgeAndDefencePercent.DefenceReduced)[0] / 100D;
            shellBoostCategory4 += GetShellArmorEffectValue(ShellOptionType.SDefenseAllPercentage) / 100D;

            //Todo: review condition
            /*if ((EntityType == EntityType.Player || EntityType == EntityType.Mate)
                && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
            {*/
                boostCategory4 += GetDefenderBenefitingBuffs(BCardType.CardType.LeonaPassiveSkill,
                                      (byte)AdditionalTypes.LeonaPassiveSkill.DefenceIncreasedInPVP)[0] / 100D;
                boostCategory4 += GetAttackerBenefitingBuffs(BCardType.CardType.LeonaPassiveSkill,
                                      (byte)AdditionalTypes.LeonaPassiveSkill.DefenceDecreasedInPVP)[0] / 100D;
                shellBoostCategory4 -=
                    GetShellWeaponEffectValue(ShellOptionType.PvpDefensePercentage) / 100D;
                shellBoostCategory4 += GetShellArmorEffectValue(ShellOptionType.PvpDefensePercentage) / 100D;
            //}

            int[] def = GetAttackerBenefitingBuffs(BCardType.CardType.Block,
                (byte)AdditionalTypes.Block.ChanceAllIncreased);
            if (ServerManager.Instance.RandomNumber() < def[0])
            {
                boostCategory3 += def[1] / 100D;
            }

            #endregion

            #endregion

            #region Type 5

            #region Static

            staticBoostCategory5 +=
                GetAttackerBenefitingBuffs(BCardType.CardType.Element, (byte)AdditionalTypes.Element.AllIncreased)[0];
            staticBoostCategory5 +=
                GetDefenderBenefitingBuffs(BCardType.CardType.Element, (byte)AdditionalTypes.Element.AllDecreased)[0];
            staticBoostCategory5 += GetShellWeaponEffectValue(ShellOptionType.SIncreaseAllElements);

            #endregion

            #region Boost

            // Nothing for now

            #endregion

            #endregion

            #region All Type Class Dependant

            int[] def2 = null;

            switch (AttackType)
            {
                case AttackType.Close:
                    def2 = GetAttackerBenefitingBuffs(BCardType.CardType.Block,
                        (byte)AdditionalTypes.Block.ChanceMeleeIncreased);
                    boostCategory1 += GetAttackerBenefitingBuffs(BCardType.CardType.Damage,
                                          (byte)AdditionalTypes.Damage.MeleeIncreased)[0] / 100D;
                    boostCategory1 += GetDefenderBenefitingBuffs(BCardType.CardType.Damage,
                                          (byte)AdditionalTypes.Damage.MeleeDecreased)[0] / 100D;
                    staticBoostCategory3 += GetAttackerBenefitingBuffs(BCardType.CardType.AttackPower,
                        (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0];
                    staticBoostCategory3 += GetDefenderBenefitingBuffs(BCardType.CardType.AttackPower,
                        (byte)AdditionalTypes.AttackPower.MeleeAttacksDecreased)[0];
                    staticBoostCategory4 += GetShellArmorEffectValue(ShellOptionType.CloseCombatDefense);
                    break;

                case AttackType.Ranged:
                    def2 = GetAttackerBenefitingBuffs(BCardType.CardType.Block,
                        (byte)AdditionalTypes.Block.ChanceRangedIncreased);
                    boostCategory1 += GetAttackerBenefitingBuffs(BCardType.CardType.Damage,
                                          (byte)AdditionalTypes.Damage.RangedIncreased)[0] / 100D;
                    boostCategory1 += GetDefenderBenefitingBuffs(BCardType.CardType.Damage,
                                          (byte)AdditionalTypes.Damage.RangedDecreased)[0] / 100D;
                    staticBoostCategory3 += GetAttackerBenefitingBuffs(BCardType.CardType.AttackPower,
                        (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0];
                    staticBoostCategory3 += GetDefenderBenefitingBuffs(BCardType.CardType.AttackPower,
                        (byte)AdditionalTypes.AttackPower.MeleeAttacksDecreased)[0];
                    staticBoostCategory4 += GetShellArmorEffectValue(ShellOptionType.LongRangeDefense);
                    break;

                case AttackType.Magical:
                    def2 = GetAttackerBenefitingBuffs(BCardType.CardType.Block,
                        (byte)AdditionalTypes.Block.ChanceRangedIncreased);
                    boostCategory1 += GetAttackerBenefitingBuffs(BCardType.CardType.Damage,
                                          (byte)AdditionalTypes.Damage.MagicalIncreased)[0] / 100D;
                    boostCategory1 += GetDefenderBenefitingBuffs(BCardType.CardType.Damage,
                                          (byte)AdditionalTypes.Damage.MagicalDecreased)[0] / 100D;
                    staticBoostCategory3 += GetAttackerBenefitingBuffs(BCardType.CardType.AttackPower,
                        (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0];
                    staticBoostCategory3 += GetDefenderBenefitingBuffs(BCardType.CardType.AttackPower,
                        (byte)AdditionalTypes.AttackPower.MeleeAttacksDecreased)[0];
                    staticBoostCategory4 += GetShellArmorEffectValue(ShellOptionType.MagicalDefense);
                    break;
            }

            if (def2 != null)
            {
                def[0] += def2[0];
                def[1] += def2[1];
            }
            #endregion

            #region Softdef finishing

            if (ServerManager.Instance.RandomNumber() < def[0])
            {
                boostCategory3 += def[1] / 100D;
            }

            #endregion

            #region Element Dependant

            switch (Element)
            {
                case 1:
                    target.FireResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    target.FireResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllDecreased)[0];
                    target.FireResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.FireIncreased)[0];
                    target.FireResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.FireDecreased)[0];
                    target.FireResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    target.FireResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0];
                    target.FireResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.FireIncreased)[0];
                    target.FireResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.FireDecreased)[0];


                    if ((EntityType == EntityType.Player || EntityType == EntityType.Mate)
                        && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
                    {
                    target.FireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedFire);
                    target.FireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedAll);
                     }

                    target.FireResistance += GetShellArmorEffectValue(ShellOptionType.FireResistanceIncrease);
                    target.FireResistance += GetShellArmorEffectValue(ShellOptionType.SIncreaseAllResistance);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellOptionType.IncreaseFireElement);
                    boostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.IncreaseDamage,
                                          (byte)AdditionalTypes.IncreaseDamage.FireIncreased)[0] / 100D;
                    staticBoostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.FireIncreased)[0];
                    staticBoostCategory5 += GetDefenderBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.FireDecreased)[0];
                    break;

                case 2:
                    target.WaterResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    target.WaterResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllDecreased)[0];
                    target.WaterResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.WaterIncreased)[0];
                    target.WaterResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.WaterDecreased)[0];
                    target.WaterResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    target.WaterResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0];
                    target.WaterResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.WaterIncreased)[0];
                    target.WaterResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.WaterDecreased)[0];

                    if ((EntityType == EntityType.Player || EntityType == EntityType.Mate)
                        && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
                    {
                    target.FireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedWater);
                    target.FireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedAll);
                    }

                    target.FireResistance += GetShellArmorEffectValue(ShellOptionType.WaterResistanceIncrease);
                    target.FireResistance += GetShellArmorEffectValue(ShellOptionType.SIncreaseAllResistance);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellOptionType.IncreaseWaterElement);
                    boostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.IncreaseDamage,
                                          (byte)AdditionalTypes.IncreaseDamage.WaterIncreased)[0] / 100D;
                    staticBoostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.WaterIncreased)[0];
                    staticBoostCategory5 += GetDefenderBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.WaterDecreased)[0];
                    break;

                case 3:
                    target.LightResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    target.LightResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllDecreased)[0];
                    target.LightResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.LightIncreased)[0];
                    target.LightResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.LightDecreased)[0];
                    target.LightResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    target.LightResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0];
                    target.LightResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.LightIncreased)[0];
                    target.LightResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.LightDecreased)[0];

                    
                    if ((EntityType == EntityType.Player || EntityType == EntityType.Mate)
                        && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
                    {
                        target.FireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedLight);
                        target.FireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedAll);
                    }

                    target.FireResistance += GetShellArmorEffectValue(ShellOptionType.LightResistanceIncrease);
                    target.FireResistance += GetShellArmorEffectValue(ShellOptionType.SIncreaseAllResistance);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellOptionType.IncreaseLightElement);
                    boostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.IncreaseDamage,
                                          (byte)AdditionalTypes.IncreaseDamage.LightIncreased)[0] / 100D;
                    staticBoostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.LightIncreased)[0];
                    staticBoostCategory5 += GetDefenderBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.LightDecreased)[0];
                    break;

                case 4:
                    target.DarkResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    target.DarkResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllDecreased)[0];
                    target.DarkResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.DarkIncreased)[0];
                    target.DarkResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.DarkDecreased)[0];
                    target.DarkResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    target.DarkResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0];
                    target.DarkResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.DarkIncreased)[0];
                    target.DarkResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.DarkDecreased)[0];
                    
                    if ((EntityType == EntityType.Player || EntityType == EntityType.Mate)
                        && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
                    {
                        target.FireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedDark);
                        target.FireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedAll);
                    }

                    target.FireResistance += GetShellArmorEffectValue(ShellOptionType.DarkResistanceIncrease);
                    target.FireResistance += GetShellArmorEffectValue(ShellOptionType.SIncreaseAllResistance);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellOptionType.IncreaseDarknessElement);
                    boostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.IncreaseDamage,
                                          (byte)AdditionalTypes.IncreaseDamage.DarkIncreased)[0] / 100D;
                    staticBoostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.DarkIncreased)[0];
                    staticBoostCategory5 += GetDefenderBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.DarkDecreased)[0];
                    break;
            }

            #endregion

            #endregion

            #region Attack Type Related Variables

            switch (AttackType)
            {
                case AttackType.Close:
                    target.Defense = target.MeleeDefense;
                    target.ArmorDefense = target.ArmorMeleeDefense;
                    target.Dodge = target.MeleeDefenseDodge;
                    break;

                case AttackType.Ranged:
                    target.Defense = target.RangeDefense;
                    target.ArmorDefense = target.ArmorRangeDefense;
                    target.Dodge = target.RangeDefenseDodge;
                    break;

                case AttackType.Magical:
                    target.Defense = target.MagicalDefense;
                    target.ArmorDefense = target.ArmorMagicalDefense;
                    break;
            }

            #endregion

            #region Too Near Range Attack Penalty (boostCategory2)

            if (AttackType == AttackType.Ranged && Map.Map.GetDistance(
                    new MapCell { X = PositionX, Y = PositionY },
                    new MapCell { X = target.PositionX, Y = target.PositionY }) < 4)
            {
                boostCategory2 -= 0.3;
            }

            #endregion

            #region Morale and Dodge

            Morale -= target.Morale;
            double chance = 0;
            if (AttackType != AttackType.Magical)
            {
                int hitrate = HitRate + Morale;
                double multiplier = target.Dodge / (hitrate > 1 ? hitrate : 1);

                if (multiplier > 5)
                {
                    multiplier = 5;
                }

                chance = (-0.25 * Math.Pow(multiplier, 3)) - (0.57 * Math.Pow(multiplier, 2)) + (25.3 * multiplier)
                         - 1.41;
                if (chance <= 1)
                {
                    chance = 1;
                }

                //if (GetBuff(CardType.Buff, (byte)AdditionalTypes.DodgeAndDefencePercent.)[0] != 0)    TODO: Eagle Eyes AND Other Fixed Hitrates
                //{
                //    chance = 10;
                //}
            }

            int bonus = 0;
            if ((EntityType == EntityType.Player || EntityType == EntityType.Mate)
                && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
            {
                switch (AttackType)
                {
                    case AttackType.Close:
                        bonus += GetShellArmorEffectValue(ShellOptionType.PvpDodgeClose);
                        break;

                    case AttackType.Ranged:
                        bonus += GetShellArmorEffectValue(ShellOptionType.PvpDodgeRanged);
                        break;

                    case AttackType.Magical:
                        bonus += GetShellArmorEffectValue(ShellOptionType.PvpDodgeMagic);
                        break;
                }

                bonus += GetShellArmorEffectValue(ShellOptionType.SPvpDodgeAll);
            }

            if (ServerManager.Instance.RandomNumber() - bonus < chance)
            {
                hitmode = 1;
                SkillBcards.Clear();
                return 0;
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.Instance.RandomNumber(MinDamage, MaxDamage + 1);
            int weaponDamage =
                ServerManager.Instance.RandomNumber(WeaponDamageMinimum, WeaponDamageMaximum + 1);

            #region Attack Level Calculation

            int[] atklvlfix = GetDefenderBenefitingBuffs(BCardType.CardType.CalculatingLevel,
                (byte)AdditionalTypes.CalculatingLevel.CalculatedAttackLevel);
            int[] deflvlfix = GetAttackerBenefitingBuffs(BCardType.CardType.CalculatingLevel,
                (byte)AdditionalTypes.CalculatingLevel.CalculatedDefenceLevel);

            if (atklvlfix[3] != 0)
            {
                AttackUpgrade = (short)atklvlfix[0];
            }

            if (deflvlfix[3] != 0)
            {
                DefenseUpgrade = (short)deflvlfix[0];
            }

            AttackUpgrade -= (short)target.DefenseUpgrade;

            if (AttackUpgrade < -10)
            {
                AttackUpgrade = -10;
            }
            else if (AttackUpgrade > 10)
            {
                AttackUpgrade = 10;
            }

            switch (AttackUpgrade)
            {
                case 0:
                    weaponDamage += 0;
                    break;

                case 1:
                    weaponDamage += (int)(weaponDamage * 0.1);
                    break;

                case 2:
                    weaponDamage += (int)(weaponDamage * 0.15);
                    break;

                case 3:
                    weaponDamage += (int)(weaponDamage * 0.22);
                    break;

                case 4:
                    weaponDamage += (int)(weaponDamage * 0.32);
                    break;

                case 5:
                    weaponDamage += (int)(weaponDamage * 0.43);
                    break;

                case 6:
                    weaponDamage += (int)(weaponDamage * 0.54);
                    break;

                case 7:
                    weaponDamage += (int)(weaponDamage * 0.65);
                    break;

                case 8:
                    weaponDamage += (int)(weaponDamage * 0.9);
                    break;

                case 9:
                    weaponDamage += (int)(weaponDamage * 1.2);
                    break;

                case 10:
                    weaponDamage += weaponDamage * 2;
                    break;

                    //default:
                    //    if (AttackUpgrade > 0)
                    //    {
                    //        weaponDamage *= AttackUpgrade / 5;
                    //    }

                    //    break;
            }

            #endregion

            baseDamage = (int)((int)((baseDamage + staticBoostCategory3 + weaponDamage + 15) * boostCategory3)
                                * shellBoostCategory3);

            #endregion

            #region Defense

            switch (AttackUpgrade)
            {
                //default:
                //    if (AttackUpgrade < 0)
                //    {
                //        target.ArmorDefense += target.ArmorDefense / 5;
                //    }

                //break;

                case -10:
                    target.ArmorDefense += target.ArmorDefense * 2;
                    break;

                case -9:
                    target.ArmorDefense += (int)(target.ArmorDefense * 1.2);
                    break;

                case -8:
                    target.ArmorDefense += (int)(target.ArmorDefense * 0.9);
                    break;

                case -7:
                    target.ArmorDefense += (int)(target.ArmorDefense * 0.65);
                    break;

                case -6:
                    target.ArmorDefense += (int)(target.ArmorDefense * 0.54);
                    break;

                case -5:
                    target.ArmorDefense += (int)(target.ArmorDefense * 0.43);
                    break;

                case -4:
                    target.ArmorDefense += (int)(target.ArmorDefense * 0.32);
                    break;

                case -3:
                    target.ArmorDefense += (int)(target.ArmorDefense * 0.22);
                    break;

                case -2:
                    target.ArmorDefense += (int)(target.ArmorDefense * 0.15);
                    break;

                case -1:
                    target.ArmorDefense += (int)(target.ArmorDefense * 0.1);
                    break;

                case 0:
                    target.ArmorDefense += 0;
                    break;
            }

            int defense =
                (int)((int)((target.Defense + target.ArmorDefense + staticBoostCategory4) * boostCategory4)
                       * shellBoostCategory4);

            if (GetAttackerBenefitingBuffs(BCardType.CardType.SpecialDefence,
                    (byte)AdditionalTypes.SpecialDefence.AllDefenceNullified)[3] != 0
                || (GetAttackerBenefitingBuffs(BCardType.CardType.SpecialDefence,
                        (byte)AdditionalTypes.SpecialDefence.MeleeDefenceNullified)[3] != 0
                    && AttackType.Equals(AttackType.Close))
                || (GetAttackerBenefitingBuffs(BCardType.CardType.SpecialDefence,
                        (byte)AdditionalTypes.SpecialDefence.RangedDefenceNullified)[3] != 0
                    && AttackType.Equals(AttackType.Ranged))
                || (GetAttackerBenefitingBuffs(BCardType.CardType.SpecialDefence,
                        (byte)AdditionalTypes.SpecialDefence.MagicDefenceNullified)[3] != 0
                    && AttackType.Equals(AttackType.Magical)))
            {
                defense = 0;
            }

            #endregion

            #region Normal Damage

            int normalDamage = (int)((int)((baseDamage + staticBoostCategory2 - defense) * boostCategory2)
                                      * shellBoostCategory2);

            if (normalDamage < 0)
            {
                normalDamage = 0;
            }

            #endregion

            #region Crit Damage

            CriticalChance += GetShellWeaponEffectValue(ShellOptionType.IncreaseCritChance);
            CriticalChance -= GetShellArmorEffectValue(ShellOptionType.ReduceCriticalChance);
            CriticalRate += GetShellWeaponEffectValue(ShellOptionType.IncreaseCritDamages);

            //Todo: Cellon options
            /*
            if (target.CellonOptions != null)
            {
                CriticalRate -= target.CellonOptions.Where(s => s.Type == CellonOptionType.CritReduce)
                    .Sum(s => s.Value);
            }
            */

            if (ServerManager.Instance.RandomNumber() < CriticalChance && AttackType != AttackType.Magical)
            {
                double multiplier = CriticalRate / 100D;
                if (multiplier > 3)
                {
                    multiplier = 3;
                }

                normalDamage += (int)(normalDamage * multiplier);
                hitmode = 3;
            }

            #endregion

            #region Fairy Damage

            int fairyDamage = (int)((baseDamage + 100) * ElementRate / 100D);

            #endregion

            #region Elemental Damage Advantage

            double elementalBoost = 0;

            switch (Element)
            {
                case 0:
                    break;

                case 1:
                    target.Resistance = target.FireResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3; // Damage vs no element
                            break;

                        case 1:
                            elementalBoost = 1; // Damage vs fire
                            break;

                        case 2:
                            elementalBoost = 2; // Damage vs water
                            break;

                        case 3:
                            elementalBoost = 1; // Damage vs light
                            break;

                        case 4:
                            elementalBoost = 1.5; // Damage vs darkness
                            break;
                    }

                    break;

                case 2:
                    target.Resistance = target.WaterResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 2;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1.5;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }

                    break;

                case 3:
                    target.Resistance = target.LightResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1.5;
                            break;

                        case 2:
                        case 3:
                            elementalBoost = 1;
                            break;

                        case 4:
                            elementalBoost = 3;
                            break;
                    }

                    break;

                case 4:
                    target.Resistance = target.DarkResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1;
                            break;

                        case 2:
                            elementalBoost = 1.5;
                            break;

                        case 3:
                            elementalBoost = 3;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }

                    break;
            }

            if (skill?.Element == 0 || (skill?.Element != Element && EntityType == EntityType.Player))
            {
                elementalBoost = 0;
            }

            #endregion

            #region Elemental Damage

            int elementalDamage =
                (int)((int)((int)((int)((staticBoostCategory5 + fairyDamage) * elementalBoost)
                                     * (1 - (target.Resistance / 100D))) * boostCategory5) * shellBoostCategory5);

            if (elementalDamage < 0)
            {
                elementalDamage = 0;
            }

            #endregion

            #region Total Damage

            int totalDamage =
                (int)((int)((normalDamage + elementalDamage + Morale + staticBoostCategory1)
                              * boostCategory1) * shellBoostCategory1);

            if ((EntityType == EntityType.Player || EntityType == EntityType.Mate)
                && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
            {
                totalDamage /= 2;
            }

            if (target.EntityType == EntityType.Monster || target.EntityType == EntityType.NPC)
            {
                totalDamage -= GetMonsterDamageBonus(target.Level);
            }

            if (totalDamage < 5)
            {
                totalDamage = ServerManager.Instance.RandomNumber(1, 6);
            }

            if (EntityType == EntityType.Monster || EntityType == EntityType.NPC)
            {
                totalDamage += GetMonsterDamageBonus(Level);
            }

            #endregion

            #region Onyx Wings

            int[] onyxBuff = GetAttackerBenefitingBuffs(BCardType.CardType.StealBuff,
                (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon);
            if (onyxBuff[0] > ServerManager.Instance.RandomNumber())
            {
                onyxEffect = true;
            }

            #endregion
            SkillBcards.Clear();
            return totalDamage;
        }

        private static int GetMonsterDamageBonus(byte level)
        {
            if (level < 45)
            {
                return 0;
            }
            else if (level < 55)
            {
                return level;
            }
            else if (level < 60)
            {
                return level * 2;
            }
            else if (level < 65)
            {
                return level * 3;
            }
            else if (level < 70)
            {
                return level * 4;
            }
            else
            {
                return level * 5;
            }
        }

        public ushort GenerateDamageOld(IBattleEntity targetEntity, Skill skill, ref int hitmode, ref bool onyxEffect)
        {
            BattleEntity target = targetEntity?.BattleEntity;
            if (target == null)
            {
                return 0;
            }

            #region Definitions

            // Percent Damage
            if (target.Session is MapMonster monster && monster.IsPercentage && monster.TakesDamage > 0)
            {
                targetEntity.DealtDamage = monster.TakesDamage;
                return (ushort)monster.TakesDamage;
            }

            AttackType attackType = Entity.GetAttackType(skill);

            int morale = Level + GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased)[0] -
                GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased)[0];
            short upgrade = AttackUpgrade;
            int critChance = CriticalChance;
            int critHit = CriticalRate;
            int minDmg = MinDamage;
            int maxDmg = MaxDamage;
            int hitRate = HitRate;

            #endregion

            #region Get Weapon Stats

            if (targetEntity is Character tChar)
            {
                //TODO: Fix reflection buffs !!!!!
                if (tChar.Buff.Any(s => s.Card.CardId == 663))
                {
                    if (ServerManager.Instance.RandomNumber() <= 20 && tChar.SpInstance?.Upgrade == 15 && tChar.CanTriggerMegaTitan)
                    {
                        tChar.AddBuff(new Buff.Buff(664));
                        tChar.LastMegaTitanBuff = DateTime.Now;
                        tChar.CanTriggerMegaTitan = false;
                        Observable.Timer(TimeSpan.FromMinutes(2)).Subscribe(s => { tChar.CanTriggerMegaTitan = true; });
                    }
                }
            }

            if (Session is Character character)
            {
                if (skill == null)
                {
                    targetEntity.DealtDamage = 0;
                    return 0;
                }

                Character toTargetChar = targetEntity is Character ? (Character)targetEntity.GetSession() : null;
                var mainWeapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                var targetHat = toTargetChar?.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.CostumeHat, InventoryType.Wear);

                if (mainWeapon != null)
                {
                    List<BCard> weaponCards = EquipmentOptionHelper.Instance.ShellToBCards(mainWeapon.EquipmentOptions.ToList(), mainWeapon.ItemVNum);
                    foreach (BCard bc in weaponCards)
                    {
                        switch ((CardType)bc.Type)
                        {
                            case CardType.Buff:
                                bc.ApplyBCards(toTargetChar, character);
                                break;
                        }
                    }
                }

                if (targetHat != null)
                {
                    foreach (BCard hatBcard in targetHat.Item.BCards)
                    {
                        switch ((CardType)hatBcard.Type)
                        {
                            case CardType.Buff:
                                hatBcard.ApplyBCards(character, toTargetChar);
                                break;
                        }
                    }
                }

                //mainWeapon.

                if (character.Buff.Any(s => s.Card.CardId == 559))
                {
                    character.TriggerAmbush = true;
                    RemoveBuff(559);
                }

                if (skill.SkillVNum == 1085) // pas de bcard ...
                {
                    character.TeleportOnMap(targetEntity.GetPos().X, targetEntity.GetPos().Y);
                }

                if (character.Inventory
                    .LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Amulet, InventoryType.Equipment)?.Item
                    ?.Effect == 932)
                {
                    upgrade += 1;
                }

                DefenceUpgrade = character.Inventory?.Armor?.Upgrade ?? 0;

                if (CharacterHelper.Instance.GetClassAttackType(character.Class) == attackType)
                {
                    minDmg += character.MinHit;
                    maxDmg += character.MaxHit;
                    hitRate += character.HitRate;
                    critChance += character.HitCriticalRate;
                    critHit += character.HitCritical;
                    upgrade += character.Inventory.PrimaryWeapon?.Upgrade ?? 0;
                }
                else
                {
                    minDmg += character.MinDistance;
                    maxDmg += character.MaxDistance;
                    hitRate += character.DistanceRate;
                    critChance += character.DistanceCriticalRate;
                    critHit += character.DistanceCritical;
                    upgrade += character.Inventory.SecondaryWeapon?.Upgrade ?? 0;
                }
            }

            #endregion

            skill?.BCards?.ToList().ForEach(s => SkillBcards.Add(s));

            #region Switch skill.Type

            int targetDefence = target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.AllIncreased)[0]
                - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.AllDecreased)[0];

            sbyte targetDefenseUpgrade =
                (sbyte)(target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelIncreased)[0]
                    - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelDecreased)[0]);

            int targetDodge = target.GetBuff(CardType.DodgeAndDefencePercent,
                    (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased)[0]
                - target.GetBuff(CardType.DodgeAndDefencePercent,
                    (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeDecreased)[0];

            int targetMorale = target.Level +
                target.GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased)[0]
                - target.GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased)[0];

            int targetBoostpercentage = 0;

            int boost = GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AllAttacksIncreased)[0]
                - GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AllAttacksDecreased)[0];

            int boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased)[0]
                - GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageDecreased)[0];

            switch (attackType)
            {
                case AttackType.Close:
                    targetDefence += target.CloseDefence;
                    targetDodge += target.DefenceRate;
                    targetBoostpercentage =
                        target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.MeleeIncreased)[0]
                        - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.MeleeDecreased)[0];
                    boost += GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0]
                        - GetBuff(CardType.AttackPower,
                            (byte)AdditionalTypes.AttackPower.MeleeAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased)[0]
                        - GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased)[0];
                    break;

                case AttackType.Ranged:
                    targetDefence += target.RangedDefence;
                    targetDodge += target.DistanceDefenceRate;
                    targetBoostpercentage =
                        target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.RangedIncreased)[0]
                        - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.RangedDecreased)[0];
                    boost += GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.RangedAttacksIncreased)[0]
                        - GetBuff(CardType.AttackPower,
                            (byte)AdditionalTypes.AttackPower.RangedAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased)[0]
                        - GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedDecreased)[0];
                    break;

                case AttackType.Magical:
                    targetDefence += target.MagicDefence;
                    targetBoostpercentage =
                        target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.MagicalIncreased)[0]
                        - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.MeleeDecreased)[0];
                    boost +=
                        GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MagicalAttacksIncreased)[0]
                        - GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MagicalAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased)[0]
                        - GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased)[0];
                    break;
            }

            targetDefence = (int)(targetDefence * (1 + targetBoostpercentage / 100D));
            minDmg += boost;
            maxDmg += boost;
            minDmg = (int)(minDmg * (1 + boostpercentage / 100D));
            maxDmg = (int)(maxDmg * (1 + boostpercentage / 100D));

            #endregion

            upgrade -= (short)((sbyte)target.DefenceUpgrade + targetDefenseUpgrade);
            #region Detailed Calculation

            #region Dodge

            if (attackType != AttackType.Magical)
            {
                double multiplier = targetDodge / (hitRate + 1);
                if (multiplier > 5)
                {
                    multiplier = 5;
                }

                double chance = -0.25 * Math.Pow(multiplier, 3) - 0.57 * Math.Pow(multiplier, 2) + 25.3 * multiplier -
                    1.41;
                if (chance <= 1)
                {
                    chance = 1;
                }

                if (GetBuff(CardType.DodgeAndDefencePercent,
                    (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased)[0] != 0)
                {
                    chance = 10;
                }

                if (skill?.Type == 0 || skill?.Type == 1)
                {
                    if (ServerManager.Instance.RandomNumber() <= chance)
                    {
                        targetEntity.DealtDamage = 0;
                        hitmode = 1;
                        SkillBcards.Clear();
                        return 0;
                    }
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.Instance.RandomNumber(minDmg, maxDmg < minDmg ? minDmg + 1 : maxDmg) +
                morale - targetMorale;

            short rest = 0;
            byte times = 0;
            if (upgrade > 10)
            {
                short upgradeCpy = upgrade;

                while (upgradeCpy - 10 > 10)
                {
                    times += 1;
                    upgradeCpy -= 10;
                }

                rest = (short)(upgradeCpy % 10);
            }
            if (upgrade < 0)
            {
                targetDefence += (int)(targetDefence * GetUpgradeValue(upgrade));
            }
            else
            {
                baseDamage += (int)(baseDamage * GetUpgradeValue((short)(upgrade > 10 ? 10 : upgrade)));
                if (times > 0)
                {
                    baseDamage += baseDamage * times * 2;
                }
                if (rest > 0)
                {
                    baseDamage += (int)(baseDamage * (GetUpgradeValue(rest)));
                }
            }

            baseDamage -=
                target.HasBuff(CardType.SpecialDefence, (byte)AdditionalTypes.SpecialDefence.AllDefenceNullified)
                    ? 0
                    : targetDefence;

            if (skill?.Type == 1 && Map.Map.GetDistance(Entity.GetPos(), targetEntity.GetPos()) < 4)
            {
                baseDamage = (int)(baseDamage * 0.85);
            }

            #endregion

            #region Elementary Damage

            #region Calculate Elemental Boost + Rate

            double elementalBoost = 0;
            int targetResistance = 0;
            int elementalDamage = GetBuff(CardType.Element, (byte)AdditionalTypes.Element.AllIncreased)[0] -
                GetBuff(CardType.Element, (byte)AdditionalTypes.Element.AllDecreased)[0];
            int bonusrez =
                target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased)[0] -
                target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllDecreased)[0];

            switch (Element)
            {
                case 1:
                    bonusrez += target.GetBuff(CardType.ElementResistance,
                            (byte)AdditionalTypes.ElementResistance.FireIncreased)[0]
                        - target.GetBuff(CardType.ElementResistance,
                            (byte)AdditionalTypes.ElementResistance.FireDecreased)[0];
                    elementalDamage += GetBuff(CardType.Element, (byte)AdditionalTypes.Element.FireIncreased)[0]
                        - GetBuff(CardType.Element, (byte)AdditionalTypes.Element.FireDecreased)[0];
                    targetResistance = target.FireResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3; // Damage vs no element
                            break;

                        case 1:
                            elementalBoost = 1; // Damage vs fire
                            break;

                        case 2:
                            elementalBoost = 2; // Damage vs water
                            break;

                        case 3:
                            elementalBoost = 1; // Damage vs light
                            break;

                        case 4:
                            elementalBoost = 1.5; // Damage vs darkness
                            break;
                    }

                    break;

                case 2:
                    bonusrez += target.GetBuff(CardType.ElementResistance,
                            (byte)AdditionalTypes.ElementResistance.WaterIncreased)[0]
                        - target.GetBuff(CardType.ElementResistance,
                            (byte)AdditionalTypes.ElementResistance.WaterDecreased)[0];
                    elementalDamage += GetBuff(CardType.Element, (byte)AdditionalTypes.Element.WaterIncreased)[0]
                        - GetBuff(CardType.Element, (byte)AdditionalTypes.Element.WaterDecreased)[0];
                    targetResistance = target.WaterResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 2;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1.5;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }

                    break;

                case 3:
                    bonusrez += target.GetBuff(CardType.ElementResistance,
                            (byte)AdditionalTypes.ElementResistance.LightIncreased)[0]
                        - target.GetBuff(CardType.ElementResistance,
                            (byte)AdditionalTypes.ElementResistance.LightDecreased)[0];
                    elementalDamage += GetBuff(CardType.Element, (byte)AdditionalTypes.Element.LightIncreased)[0]
                        - GetBuff(CardType.Element, (byte)AdditionalTypes.Element.LightDecreased)[0];
                    targetResistance = target.LightResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1.5;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1;
                            break;

                        case 4:
                            elementalBoost = 3;
                            break;
                    }

                    break;

                case 4:
                    bonusrez += target.GetBuff(CardType.ElementResistance,
                            (byte)AdditionalTypes.ElementResistance.DarkIncreased)[0]
                        - target.GetBuff(CardType.ElementResistance,
                            (byte)AdditionalTypes.ElementResistance.DarkDecreased)[0];
                    targetResistance = target.DarkResistance;
                    elementalDamage += GetBuff(CardType.Element, (byte)AdditionalTypes.Element.DarkIncreased)[0]
                        - GetBuff(CardType.Element, (byte)AdditionalTypes.Element.DarkDecreased)[0];
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1;
                            break;

                        case 2:
                            elementalBoost = 1.5;
                            break;

                        case 3:
                            elementalBoost = 3;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }

                    break;
            }

            #endregion;

            if (skill?.Element == 0)
            {
                switch (elementalBoost)
                {
                    case 0.5:
                        elementalBoost = 0;
                        break;

                    case 1:
                        elementalBoost = 0.05;
                        break;

                    case 1.3:
                    case 1.5:
                        elementalBoost = 0.15;
                        break;

                    case 2:
                    case 3:
                        elementalBoost = 0.2;
                        break;
                }
            }
            else if (skill?.Element != Element)
            {
                elementalBoost = 0;
            }

            int resistance = targetResistance + bonusrez;
            elementalDamage = (int)(elementalDamage + (baseDamage + 100) * ((ElementRate + ElementRateSp) / 100D));
            elementalDamage = (int)(elementalDamage / 100D * (100 - (resistance > 100 ? 100 : resistance)) *
                elementalBoost);

            #endregion

            #region Critical Damage

            critChance += GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.InflictingIncreased)[0]
                - GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.InflictingReduced)[0];

            critHit += GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreased)[0]
                - GetBuff(CardType.Critical,
                    (byte)AdditionalTypes.Critical.DamageIncreasedInflictingReduced)[0];

            if (ServerManager.Instance.RandomNumber() <= critChance || targetEntity.HasBuff(CardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.AlwaysReceives))
            {
                if (skill?.Type != 2 && attackType != AttackType.Magical)
                {
                    double multiplier = critHit / 100D;
                    multiplier = multiplier > 3 ? 3 : multiplier;
                    baseDamage += (int)(baseDamage * multiplier);
                    hitmode = 3;
                }
            }

            #endregion

            // OFFENSIVE POTION
            baseDamage += (int)(baseDamage * GetBuff(CardType.Item, (byte)AdditionalTypes.Item.AttackIncreased)[0] /
                100D);

            if (Session is Character charact)
            {
                var weapon =
                    charact.Inventory.LoadBySlotAndType<WearableInstance>((short)EquipmentType.MainWeapon,
                        InventoryType.Wear);
                if (weapon != null)
                {
                    foreach (BCard bcard in weapon.Item.BCards)
                    {
                        var b = new Buff.Buff(bcard.SecondData);
                        switch (b.Card?.BuffType)
                        {
                            case BuffType.Good:
                                bcard.ApplyBCards(charact, charact);
                                break;
                            case BuffType.Bad:
                                bcard.ApplyBCards(targetEntity, charact);
                                break;
                        }
                    }
                }

                int[] weaponSoftDamage = charact.GetWeaponSoftDamage();
                if (ServerManager.Instance.RandomNumber() < weaponSoftDamage[0])
                {
                    charact.MapInstance.Broadcast(charact.GenerateEff(15));
                    baseDamage += (int)(baseDamage * (1 + weaponSoftDamage[1] / 100D));
                }

                if (charact.HasBuff(CardType.IncreaseDamage,
                    (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, true))
                {
                    charact.MapInstance.Broadcast(charact.GenerateEff(15));
                    baseDamage += (int)(baseDamage * (1 + GetBuff(CardType.IncreaseDamage,
                        (byte)AdditionalTypes.IncreaseDamage
                            .IncreasingPropability)[0] / 100D));
                }

                // Falcon invisibility
                if (charact.Buff.Any(s => s.Card.CardId == 559))
                {
                    RemoveBuff(559);
                    charact.AddBuff(new Buff.Buff(560));
                }

                if (charact.ChargeValue > 0)
                {
                    baseDamage += charact.ChargeValue;
                    charact.ChargeValue = 0;
                    charact.RemoveBuff(0);
                }

                baseDamage += charact.Class == ClassType.Adventurer ? 20 : 0;
            }

            #region Total Damage

            int totalDamage = baseDamage + elementalDamage;
            totalDamage = totalDamage < 5 ? ServerManager.Instance.RandomNumber(1, 6) : totalDamage;

            #endregion

            if (Session is MapMonster)
            {
                if (Level < 45)
                {
                    //no minimum damage
                }
                else if (Level < 55)
                {
                    totalDamage += Level;
                }
                else if (Level < 60)
                {
                    totalDamage += Level * 2;
                }
                else if (Level < 65)
                {
                    totalDamage += Level * 3;
                }
                else if (Level < 70)
                {
                    totalDamage += Level * 4;
                }
                else
                {
                    totalDamage += Level * 5;
                }
            }

            if (targetEntity.GetSession() is Character chara && target.HasBuff(CardType.NoDefeatAndNoDamage,
                (byte)AdditionalTypes.NoDefeatAndNoDamage.TransferAttackPower))
            {
                chara.ChargeValue = totalDamage;
                chara.AddBuff(new Buff.Buff(0));
                totalDamage = 0;
                hitmode = 1;
            }

            #endregion

            targetEntity.DealtDamage = totalDamage;

            Character targetChar = targetEntity is Character ? (Character)targetEntity.GetSession() : null;


            if (Session is Character currentChar)
            {
                var targetCostume = targetChar?.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.CostumeSuit, InventoryType.Wear);
                if (targetCostume != null)
                {
                    foreach (BCard costumeBcard in targetCostume.Item.BCards)
                    {
                        switch ((CardType)costumeBcard.Type)
                        {
                            case CardType.Buff:
                                costumeBcard.ApplyBCards(currentChar, targetChar);
                                break;
                            case CardType.Block:
                                switch (costumeBcard.SubType)
                                {
                                    case (byte)AdditionalTypes.Block.ChanceAllIncreased:
                                        if (ServerManager.Instance.RandomNumber() < costumeBcard.FirstData)
                                        {
                                            totalDamage = (int)(totalDamage * 0.2);
                                            targetEntity.DealtDamage = (int)(targetEntity.DealtDamage * 0.2);
                                        }
                                        break;
                                }
                                break;
                            case CardType.EffectSummon:
                                if (ServerManager.Instance.RandomNumber() < costumeBcard.FirstData)
                                {
                                    targetChar.AddBuff(new Buff.Buff(costumeBcard.SecondData));
                                }
                                break;
                        }
                    }
                }
            }


            while (totalDamage > ushort.MaxValue)
            {
                totalDamage -= ushort.MaxValue;
            }

            #region Onyx Wings

            onyxEffect = GetBuff(CardType.StealBuff, (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon)[0] >
                ServerManager.Instance.RandomNumber();

            #endregion

            SkillBcards.Clear();
            if (Session is Character charac && targetEntity is MapMonster cali && cali.MonsterVNum == 2305 &&
                Caligor.IsRunning)
            {
                switch (charac.Faction)
                {
                    case FactionType.Angel:
                        Caligor.AngelDamage +=
                            targetEntity.DealtDamage + (onyxEffect ? targetEntity.DealtDamage / 2 : 0);
                        break;
                    case FactionType.Demon:
                        Caligor.DemonDamage +=
                            targetEntity.DealtDamage + (onyxEffect ? targetEntity.DealtDamage / 2 : 0);
                        break;
                }
            }

            return (ushort)totalDamage;
        }

        public int[] GetBuff(CardType type, byte subtype)
        {
            int value1 = 0;
            int value2 = 0;

            foreach (BCard entry in StaticBcards.Concat(SkillBcards)
                .Where(s => s != null && s.Type.Equals((byte)type) && s.SubType.Equals(subtype)))
            {
                value1 += entry.IsLevelScaled
                    ? entry.IsLevelDivided ? Level / entry.FirstData : entry.FirstData * Level
                    : entry.FirstData;
                value2 += entry.SecondData;
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
            return false;
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
            int damage = GenerateDamage(target, skill, ref hitmode, ref onyxWings);

            if (Session is Character charact && mapInstance != null)
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

            if (!isBoss && skill != null)
            {
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