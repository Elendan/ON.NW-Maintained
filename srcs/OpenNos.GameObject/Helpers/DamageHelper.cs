using System;
using System.Collections.Generic;
using System.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.GameObject.Battle;
using OpenNos.GameObject.Buff;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Helpers
{
    public class DamageHelper
    {
        #region Properties
        #endregion

        #region Methods

        private static int[] GetBuff(byte level, IReadOnlyCollection<Buff.Buff> buffs, IReadOnlyCollection<BCard> bcards, BCardType.CardType type,
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
                        s.Type.Equals((byte)type) && s.SubType.Equals((subtype)) && s.FirstData >= 0)
                    : bcards.Where(s =>
                        s.Type.Equals((byte)type) && s.SubType.Equals((subtype))
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
                            s.Type.Equals((byte)type) && s.SubType.Equals((subtype))
                            && (s.CastType != 1 || (s.CastType == 1
                                                 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now))
                            && s.FirstData >= 0)
                        : buff.Card.BCards.Where(s =>
                            s.Type.Equals((byte)type) && s.SubType.Equals((subtype))
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

        public void DefineAttackType(IBattleEntity session, Skill skill)
        {
            WearableInstance weapon = null;

            if (session is Character character)
            {
                character.BattleEntity.Morale = character.Level;
                character.BattleEntity.EntityType = EntityType.Player;
                character.BattleEntity.MinDamage = character.MinHit;
                character.BattleEntity.MaxDamage = character.MaxHit;
                character.BattleEntity.CriticalChance = character.HitCriticalRate;
                character.BattleEntity.CriticalRate = character.HitCritical;
                character.BattleEntity.DarkResistance = character.DarkResistance;
                character.BattleEntity.LightResistance = character.LightResistance;
                character.BattleEntity.WaterResistance = character.WaterResistance;
                character.BattleEntity.FireResistance = character.FireResistance;
                character.BattleEntity.ShellOptionsMain = character.ShellOptionsMain;
                character.BattleEntity.ShellOptionsSecondary = character.ShellOptionsSecondary;
                character.BattleEntity.ShellOptionArmor = character.ShellOptionArmor;
                character.BattleEntity.Morale = character.Level;
                character.BattleEntity.PositionX = character.PositionX;
                character.BattleEntity.PositionY = character.PositionY;
                if (skill != null)
                {
                    switch (skill.Type)
                    {
                        case 0:
                            character.BattleEntity.AttackType = AttackType.Close;
                            if (character.Class == ClassType.Archer)
                            {
                                character.BattleEntity.MinDamage = character.MinDistance;
                                character.BattleEntity.MaxDamage = character.MaxDistance;
                                character.BattleEntity.HitRate = character.DistanceRate;
                                character.BattleEntity.CriticalChance = character.DistanceCriticalRate;
                                character.BattleEntity.CriticalRate = character.DistanceCritical;
                                weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                            }
                            else
                            {
                                weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                            }
                            break;

                        case 1:
                            character.BattleEntity.AttackType = AttackType.Ranged;
                            if (character.Class == ClassType.Adventurer || character.Class == ClassType.Swordman || character.Class == ClassType.Magician)
                            {
                                character.BattleEntity.MinDamage = character.MinDistance;
                                character.BattleEntity.MaxDamage = character.MaxDistance;
                                character.BattleEntity.HitRate = character.DistanceRate;
                                character.BattleEntity.CriticalChance = character.DistanceCriticalRate;
                                character.BattleEntity.CriticalRate = character.DistanceCritical;
                                weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                            }
                            else
                            {
                                weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                            }
                            break;

                        case 2:
                            character.BattleEntity.AttackType = AttackType.Magical;
                            weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                            break;

                        case 3:
                            weapon = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                            switch (character.Class)
                            {
                                case ClassType.Adventurer:
                                case ClassType.Swordman:
                                    character.BattleEntity.AttackType = AttackType.Close;
                                    break;

                                case ClassType.Archer:
                                    character.BattleEntity.AttackType = AttackType.Ranged;
                                    break;

                                case ClassType.Magician:
                                    character.BattleEntity.AttackType = AttackType.Magical;
                                    break;
                            }
                            break;

                        case 5:
                            character.BattleEntity.AttackType = AttackType.Close;
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
                            character.BattleEntity.AttackType = AttackType.Close;
                            break;

                        case ClassType.Archer:
                            character.BattleEntity.AttackType = AttackType.Ranged;
                            break;

                        case ClassType.Magician:
                            character.BattleEntity.AttackType = AttackType.Magical;
                            break;
                    }
                }

                if (weapon != null)
                {
                    character.BattleEntity.AttackUpgrade = weapon.Upgrade;
                    character.BattleEntity.WeaponDamageMinimum = weapon.DamageMinimum + weapon.Item.DamageMinimum;
                    character.BattleEntity.WeaponDamageMaximum = weapon.DamageMaximum + weapon.Item.DamageMinimum;
                }

                var armor = character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Armor, InventoryType.Wear);
                if (armor != null)
                {
                    character.BattleEntity.DefenseUpgrade = armor.Upgrade;
                    character.BattleEntity.ArmorMeleeDefense = armor.CloseDefence + armor.Item.CloseDefence;
                    character.BattleEntity.ArmorRangeDefense = armor.DistanceDefence + armor.Item.DistanceDefence;
                    character.BattleEntity.ArmorMagicalDefense = armor.MagicDefence + armor.Item.MagicDefence;
                }

                character.BattleEntity.MeleeDefense = character.Defence - character.BattleEntity.ArmorMeleeDefense;
                character.BattleEntity.MeleeDefenseDodge = character.DefenceRate;
                character.BattleEntity.RangeDefense = character.DistanceDefence - character.BattleEntity.ArmorRangeDefense;
                character.BattleEntity.RangeDefenseDodge = character.DistanceDefenceRate;
                character.BattleEntity.MagicalDefense = character.MagicalDefence - character.BattleEntity.ArmorMagicalDefense;
                character.BattleEntity.Element = character.Element;
            }
            else if (session is Mate mate)
            {
                mate.BattleEntity.Morale = mate.Level;

                mate.BattleEntity.HpMax = mate.MaxHp;
                mate.BattleEntity.MpMax = mate.MaxMp;

                var mateWeapon = (WearableInstance)mate.WeaponInstance;
                var mateArmor = (WearableInstance)mate.ArmorInstance;
                var mateGloves = (WearableInstance)mate.GlovesInstance;
                var mateBoots = (WearableInstance)mate.BootsInstance;

                mate.BattleEntity.Buffs = mate.Buffs;
                //BCards = mate.Monster.BCards.ToList();
                mate.BattleEntity.Level = mate.Level;
                mate.BattleEntity.EntityType = EntityType.Mate;
                mate.BattleEntity.MinDamage = (mateWeapon?.DamageMinimum ?? 0) /*+ mate.BaseDamage*/ + mate.Monster.DamageMinimum;
                mate.BattleEntity.MaxDamage = (mateWeapon?.DamageMaximum ?? 0) /*+ mate.BaseDamage */ + mate.Monster.DamageMaximum;
                mate.BattleEntity.WeaponDamageMinimum = (mateWeapon?.DamageMinimum ?? mate.BattleEntity.MinDamage);
                mate.BattleEntity.WeaponDamageMaximum = (mateWeapon?.DamageMaximum ?? mate.BattleEntity.MaxDamage);
                mate.BattleEntity.PositionX = mate.PositionX;
                mate.BattleEntity.PositionY = mate.PositionY;
                mate.BattleEntity.HitRate = mate.Monster.Concentrate + (mateWeapon?.HitRate ?? 0);
                mate.BattleEntity.CriticalChance = mate.Monster.CriticalChance + (mateWeapon?.CriticalLuckRate ?? 0);
                mate.BattleEntity.CriticalRate = mate.Monster.CriticalRate + (mateWeapon?.CriticalRate ?? 0);
                mate.BattleEntity.Morale = mate.Level;
                mate.BattleEntity.AttackUpgrade = mateWeapon?.Upgrade ?? mate.Attack;
                mate.BattleEntity.FireResistance = mate.Monster.FireResistance + (mateGloves?.FireResistance ?? 0) + (mateBoots?.FireResistance ?? 0);
                mate.BattleEntity.WaterResistance = mate.Monster.WaterResistance + (mateGloves?.FireResistance ?? 0) + (mateBoots?.FireResistance ?? 0);
                mate.BattleEntity.LightResistance = mate.Monster.LightResistance + (mateGloves?.FireResistance ?? 0) + (mateBoots?.FireResistance ?? 0);
                mate.BattleEntity.DarkResistance = mate.Monster.DarkResistance + (mateGloves?.FireResistance ?? 0) + (mateBoots?.FireResistance ?? 0);
                mate.BattleEntity.AttackType = (AttackType)mate.Monster.AttackClass;

                mate.BattleEntity.DefenseUpgrade = mateArmor?.Upgrade ?? mate.Defence;
                mate.BattleEntity.MeleeDefense = (mateArmor?.CloseDefence ?? 0)/* + mate.MeleeDefense */ + mate.Monster.CloseDefence;
                mate.BattleEntity.RangeDefense = (mateArmor?.DistanceDefence ?? 0) /*+ mate.RangeDefense */ + mate.Monster.DistanceDefence;
                mate.BattleEntity.MagicalDefense = (mateArmor?.MagicDefence ?? 0) /*+ mate.MagicalDefense */ + mate.Monster.MagicDefence;
                mate.BattleEntity.MeleeDefenseDodge = (mateArmor?.DefenceDodge ?? 0) /*+ mate.MeleeDefenseDodge */ + mate.Monster.DefenceDodge;
                mate.BattleEntity.RangeDefenseDodge = (mateArmor?.DistanceDefenceDodge ?? 0) /*+ mate.RangeDefenseDodge */ + mate.Monster.DistanceDefenceDodge;

                mate.BattleEntity.ArmorMeleeDefense = mateArmor?.CloseDefence ?? mate.BattleEntity.MeleeDefense;
                mate.BattleEntity.ArmorRangeDefense = mateArmor?.DistanceDefence ?? mate.BattleEntity.RangeDefense;
                mate.BattleEntity.ArmorMagicalDefense = mateArmor?.MagicDefence ?? mate.BattleEntity.MagicalDefense;

                mate.BattleEntity.Element = mate.Monster.Element;
                mate.BattleEntity.ElementRate = mate.Monster.ElementRate;
            }
            else if (session is MapMonster monster)
            {
                monster.BattleEntity.Morale = monster.Monster.Level;

                monster.BattleEntity.HpMax = monster.Monster.MaxHP;
                monster.BattleEntity.MpMax = monster.Monster.MaxMP;
                monster.BattleEntity.Buffs = monster.Buffs;
                //BCards = monster.Monster.BCards.ToList();
                monster.BattleEntity.Level = monster.Monster.Level;
                monster.BattleEntity.EntityType = EntityType.Monster;
                monster.BattleEntity.MinDamage = 0;
                monster.BattleEntity.MaxDamage = 0;
                monster.BattleEntity.WeaponDamageMinimum = monster.Monster.DamageMinimum;
                monster.BattleEntity.WeaponDamageMaximum = monster.Monster.DamageMaximum;
                monster.BattleEntity.HitRate = monster.Monster.Concentrate;
                monster.BattleEntity.CriticalChance = monster.Monster.CriticalChance;
                monster.BattleEntity.CriticalRate = monster.Monster.CriticalRate;
                monster.BattleEntity.Morale = monster.Monster.Level;
                monster.BattleEntity.AttackUpgrade = monster.Monster.AttackUpgrade;
                monster.BattleEntity.FireResistance = monster.Monster.FireResistance;
                monster.BattleEntity.WaterResistance = monster.Monster.WaterResistance;
                monster.BattleEntity.LightResistance = monster.Monster.LightResistance;
                monster.BattleEntity.DarkResistance = monster.Monster.DarkResistance;
                monster.BattleEntity.PositionX = monster.MapX;
                monster.BattleEntity.PositionY = monster.MapY;
                monster.BattleEntity.AttackType = (AttackType)monster.Monster.AttackClass;
                monster.BattleEntity.DefenseUpgrade = monster.Monster.DefenceUpgrade;
                monster.BattleEntity.MeleeDefense = monster.Monster.CloseDefence;
                monster.BattleEntity.MeleeDefenseDodge = monster.Monster.DefenceDodge;
                monster.BattleEntity.RangeDefense = monster.Monster.DistanceDefence;
                monster.BattleEntity.RangeDefenseDodge = monster.Monster.DistanceDefenceDodge;
                monster.BattleEntity.MagicalDefense = monster.Monster.MagicDefence;
                monster.BattleEntity.ArmorMeleeDefense = monster.Monster.CloseDefence;
                monster.BattleEntity.ArmorRangeDefense = monster.Monster.DistanceDefence;
                monster.BattleEntity.ArmorMagicalDefense = monster.Monster.MagicDefence;
                monster.BattleEntity.Element = monster.Monster.Element;
                monster.BattleEntity.ElementRate = monster.Monster.ElementRate;
            }
            else if (session is MapNpc npc)
            {
                npc.BattleEntity.Morale = npc.Npc.Level;

                npc.BattleEntity.HpMax = npc.Npc.MaxHP;
                npc.BattleEntity.MpMax = npc.Npc.MaxMP;

                //npc.Buff.CopyTo(Buffs);
                //BCards = npc.Npc.BCards.ToList();
                npc.BattleEntity.Level = npc.Npc.Level;
                npc.BattleEntity.EntityType = EntityType.Monster;
                npc.BattleEntity.MinDamage = 0;
                npc.BattleEntity.MaxDamage = 0;
                npc.BattleEntity.WeaponDamageMinimum = npc.Npc.DamageMinimum;
                npc.BattleEntity.WeaponDamageMaximum = npc.Npc.DamageMaximum;
                npc.BattleEntity.HitRate = npc.Npc.Concentrate;
                npc.BattleEntity.CriticalChance = npc.Npc.CriticalChance;
                npc.BattleEntity.CriticalRate = npc.Npc.CriticalRate;
                npc.BattleEntity.Morale = npc.Npc.Level;
                npc.BattleEntity.AttackUpgrade = npc.Npc.AttackUpgrade;
                npc.BattleEntity.FireResistance = npc.Npc.FireResistance;
                npc.BattleEntity.WaterResistance = npc.Npc.WaterResistance;
                npc.BattleEntity.LightResistance = npc.Npc.LightResistance;
                npc.BattleEntity.DarkResistance = npc.Npc.DarkResistance;
                npc.BattleEntity.PositionX = npc.MapX;
                npc.BattleEntity.PositionY = npc.MapY;
                npc.BattleEntity.AttackType = (AttackType)npc.Npc.AttackClass;
                npc.BattleEntity.DefenseUpgrade = npc.Npc.DefenceUpgrade;
                npc.BattleEntity.MeleeDefense = npc.Npc.CloseDefence;
                npc.BattleEntity.MeleeDefenseDodge = npc.Npc.DefenceDodge;
                npc.BattleEntity.RangeDefense = npc.Npc.DistanceDefence;
                npc.BattleEntity.RangeDefenseDodge = npc.Npc.DistanceDefenceDodge;
                npc.BattleEntity.MagicalDefense = npc.Npc.MagicDefence;
                npc.BattleEntity.ArmorMeleeDefense = npc.Npc.CloseDefence;
                npc.BattleEntity.ArmorRangeDefense = npc.Npc.DistanceDefence;
                npc.BattleEntity.ArmorMagicalDefense = npc.Npc.MagicDefence;
                npc.BattleEntity.Element = npc.Npc.Element;
                npc.BattleEntity.ElementRate = npc.Npc.ElementRate;
            }

        }

        public int GenerateDamage(BattleEntity attacker, IBattleEntity targetEntity, Skill skill, ref int hitmode, ref bool onyxEffect)
        {
            BattleEntity target = targetEntity?.BattleEntity;
            DefineAttackType(attacker.Entity, skill);

            if (target == null)
            {
                return 0;
            }

            if (attacker.Entity is Character teleported && skill.SkillVNum == 1085) // Pas de bcard
            {
                teleported.TeleportOnMap(target.PositionX, target.PositionY);
            }

            int[] GetAttackerBenefitingBuffs(BCardType.CardType type, byte subtype)
            {
                int value1 = 0;
                int value2 = 0;
                int value3 = 0;
                int temp = 0;

                int[] tmp = GetBuff(attacker.Level, attacker.Buffs, attacker.StaticBcards, type, subtype, BuffType.Good,
                    ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];
                tmp = GetBuff(attacker.Level, attacker.Buffs, attacker.StaticBcards, type, subtype, BuffType.Neutral,
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
                tmp = GetBuff(attacker.Level, attacker.Buffs, attacker.StaticBcards, type, subtype, BuffType.Bad, ref temp);
                value1 += tmp[0];
                value2 += tmp[1];
                value3 += tmp[2];

                return new[] { value1, value2, value3, temp };
            }

            int GetShellWeaponEffectValue(ShellOptionType effectType)
            {
                return attacker.ShellOptionsMain.FirstOrDefault(s => s.Type == (byte)effectType)?.Value ?? 0;
            }

            int GetShellArmorEffectValue(ShellOptionType effectType, bool isTarget = false)
            {
                if (isTarget)
                {
                    return target.ShellOptionArmor.FirstOrDefault(s => s.Type == (byte)effectType)?.Value ?? 0;
                }
                return attacker.ShellOptionArmor.FirstOrDefault(s => s.Type == (byte)effectType)?.Value ?? 0;
            }

            if (targetEntity is Character character)
            {
                targetEntity.BattleEntity.ShellOptionsMain = character.ShellOptionsMain;
                targetEntity.BattleEntity.ShellOptionsSecondary = character.ShellOptionsSecondary;
                targetEntity.BattleEntity.ShellOptionArmor = character.ShellOptionArmor;
                if (character.HasGodMode)
                {
                    targetEntity.DealtDamage = 0;
                    return 0;
                }
            }

            if (attacker.Entity is Character charactersession)
            {
                if (charactersession.Buff.Any(s => s.Card.CardId == 559))
                {
                    charactersession.TriggerAmbush = true;
                    attacker.RemoveBuff(559);
                }
            }

            skill?.BCards.ToList().ForEach(s => attacker.SkillBcards.Add(s));

            #region Basic Buff Initialisation

            int morale = attacker.Morale;
            int targetMorale = target.Morale;
            int attackUpgrade = attacker.AttackUpgrade;
            int targetDefenseUpgrade = target.DefenseUpgrade;

            morale +=
                GetAttackerBenefitingBuffs(BCardType.CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased)[0];
            morale +=
                GetDefenderBenefitingBuffs(BCardType.CardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased)[0];
            targetMorale +=
                GetDefenderBenefitingBuffs(BCardType.CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased)[0];
            targetMorale +=
                GetAttackerBenefitingBuffs(BCardType.CardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased)[0];

            attackUpgrade += (byte)GetAttackerBenefitingBuffs(BCardType.CardType.AttackPower,
                (byte)AdditionalTypes.AttackPower.AttackLevelIncreased)[0];
            attackUpgrade += (byte)GetDefenderBenefitingBuffs(BCardType.CardType.AttackPower,
                (byte)AdditionalTypes.AttackPower.AttackLevelDecreased)[0];
            targetDefenseUpgrade += (byte)GetDefenderBenefitingBuffs(BCardType.CardType.Defence,
                (byte)AdditionalTypes.Defence.DefenceLevelIncreased)[0];
            targetDefenseUpgrade += (byte)GetAttackerBenefitingBuffs(BCardType.CardType.Defence,
                (byte)AdditionalTypes.Defence.DefenceLevelDecreased)[0];

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

            if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
            {
                boostCategory1 += GetAttackerBenefitingBuffs(BCardType.CardType.SpecialisationBuffResistance,
                                      (byte)AdditionalTypes.SpecialisationBuffResistance.IncreaseDamageInPVP)[0]
                                  / 100D;
                boostCategory1 += GetAttackerBenefitingBuffs(BCardType.CardType.LeonaPassiveSkill,
                                      (byte)AdditionalTypes.LeonaPassiveSkill.AttackIncreasedInPVP)[0] / 100D;
                shellBoostCategory1 += GetShellWeaponEffectValue(ShellOptionType.PvpDamagePercentage) / 100D;
            }

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

            if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
            {
                boostCategory2 += GetDefenderBenefitingBuffs(BCardType.CardType.SpecialisationBuffResistance,
                        (byte)AdditionalTypes.SpecialisationBuffResistance.DecreaseDamageInPVP)[0]
                    / 100D;
                boostCategory2 += GetDefenderBenefitingBuffs(BCardType.CardType.LeonaPassiveSkill,
                    (byte)AdditionalTypes.LeonaPassiveSkill.AttackDecreasedInPVP)[0] / 100D;
            }

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
            if (attacker.Level < target.Level)
            {
                soft[0] += skin[0];
                soft[1] += skin[1];
            }

            if (ServerManager.Instance.RandomNumber() < soft[0])
            {
                boostCategory3 += soft[1] / 100D;
                if (attacker.Entity is Character c)
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

            if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
            {
                boostCategory4 += GetDefenderBenefitingBuffs(BCardType.CardType.LeonaPassiveSkill,
                                      (byte)AdditionalTypes.LeonaPassiveSkill.DefenceIncreasedInPVP)[0] / 100D;
                boostCategory4 += GetAttackerBenefitingBuffs(BCardType.CardType.LeonaPassiveSkill,
                                      (byte)AdditionalTypes.LeonaPassiveSkill.DefenceDecreasedInPVP)[0] / 100D;
                shellBoostCategory4 -=
                    GetShellWeaponEffectValue(ShellOptionType.PvpDefensePercentage) / 100D;
                shellBoostCategory4 += GetShellArmorEffectValue(ShellOptionType.PvpDefensePercentage) / 100D;
            }

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

            switch (attacker.AttackType)
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

            int targetFireResistance = target.FireResistance;
            int targetWaterResistance = target.WaterResistance;
            int targetLightResistance = target.LightResistance;
            int targetDarkResistance = target.DarkResistance;

            switch (attacker.Element)
            {
                case 1:
                    targetFireResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    targetFireResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllDecreased)[0];
                    targetFireResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.FireIncreased)[0];
                    targetFireResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.FireDecreased)[0];
                    targetFireResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    targetFireResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0];
                    targetFireResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.FireIncreased)[0];
                    targetFireResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.FireDecreased)[0];


                    if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                        && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
                    {
                        targetFireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedFire);
                        targetFireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedAll);
                    }

                    targetFireResistance += GetShellArmorEffectValue(ShellOptionType.FireResistanceIncrease);
                    targetFireResistance += GetShellArmorEffectValue(ShellOptionType.SIncreaseAllResistance);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellOptionType.IncreaseFireElement);
                    boostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.IncreaseDamage,
                                          (byte)AdditionalTypes.IncreaseDamage.FireIncreased)[0] / 100D;
                    staticBoostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.FireIncreased)[0];
                    staticBoostCategory5 += GetDefenderBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.FireDecreased)[0];
                    break;

                case 2:
                    targetWaterResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    targetWaterResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllDecreased)[0];
                    targetWaterResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.WaterIncreased)[0];
                    targetWaterResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.WaterDecreased)[0];
                    targetWaterResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    targetWaterResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0];
                    targetWaterResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.WaterIncreased)[0];
                    targetWaterResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.WaterDecreased)[0];

                    if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                        && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
                    {
                        targetFireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedWater);
                        targetFireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedAll);
                    }

                    targetFireResistance += GetShellArmorEffectValue(ShellOptionType.WaterResistanceIncrease);
                    targetFireResistance += GetShellArmorEffectValue(ShellOptionType.SIncreaseAllResistance);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellOptionType.IncreaseWaterElement);
                    boostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.IncreaseDamage,
                                          (byte)AdditionalTypes.IncreaseDamage.WaterIncreased)[0] / 100D;
                    staticBoostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.WaterIncreased)[0];
                    staticBoostCategory5 += GetDefenderBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.WaterDecreased)[0];
                    break;

                case 3:
                    targetLightResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    targetLightResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllDecreased)[0];
                    targetLightResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.LightIncreased)[0];
                    targetLightResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.LightDecreased)[0];
                    targetLightResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    targetLightResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0];
                    targetLightResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.LightIncreased)[0];
                    targetLightResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.LightDecreased)[0];


                    if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                        && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
                    {
                        targetFireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedLight);
                        targetFireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedAll);
                    }

                    targetFireResistance += GetShellArmorEffectValue(ShellOptionType.LightResistanceIncrease);
                    targetFireResistance += GetShellArmorEffectValue(ShellOptionType.SIncreaseAllResistance);
                    staticBoostCategory5 += GetShellWeaponEffectValue(ShellOptionType.IncreaseLightElement);
                    boostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.IncreaseDamage,
                                          (byte)AdditionalTypes.IncreaseDamage.LightIncreased)[0] / 100D;
                    staticBoostCategory5 += GetAttackerBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.LightIncreased)[0];
                    staticBoostCategory5 += GetDefenderBenefitingBuffs(BCardType.CardType.Element,
                        (byte)AdditionalTypes.Element.LightDecreased)[0];
                    break;

                case 4:
                    targetDarkResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
                    targetDarkResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.AllDecreased)[0];
                    targetDarkResistance += GetDefenderBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.DarkIncreased)[0];
                    targetDarkResistance += GetAttackerBenefitingBuffs(BCardType.CardType.ElementResistance,
                        (byte)AdditionalTypes.ElementResistance.DarkDecreased)[0];
                    targetDarkResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllIncreased)[0];
                    targetDarkResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.AllDecreased)[0];
                    targetDarkResistance += GetDefenderBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.DarkIncreased)[0];
                    targetDarkResistance += GetAttackerBenefitingBuffs(BCardType.CardType.EnemyElementResistance,
                        (byte)AdditionalTypes.EnemyElementResistance.DarkDecreased)[0];

                    if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                        && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
                    {
                        targetFireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedDark);
                        targetFireResistance -=
                            GetShellWeaponEffectValue(ShellOptionType.PvpResistanceDecreasedAll);
                    }

                    targetFireResistance += GetShellArmorEffectValue(ShellOptionType.DarkResistanceIncrease);
                    targetFireResistance += GetShellArmorEffectValue(ShellOptionType.SIncreaseAllResistance);
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

            switch (attacker.AttackType)
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

            if (attacker.AttackType == AttackType.Ranged && Map.Map.GetDistance(
                    new MapCell { X = attacker.PositionX, Y = attacker.PositionY },
                    new MapCell { X = target.PositionX, Y = target.PositionY }) < 4)
            {
                boostCategory2 -= 0.3;
            }

            #endregion

            #region Morale and Dodge

            morale -= targetMorale;
            double chance = 0;
            if (attacker.AttackType != AttackType.Magical)
            {
                int hitrate = attacker.HitRate + morale;
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
            }

            double bonus = 0;
            double magicBonus = 0;
            if ((attacker.EntityType == EntityType.Player || attacker.EntityType == EntityType.Mate)
                && (target.EntityType == EntityType.Player || target.EntityType == EntityType.Mate))
            {
                switch (attacker.AttackType)
                {
                    case AttackType.Close:
                        bonus += GetShellArmorEffectValue(ShellOptionType.PvpDodgeClose, true);
                        break;

                    case AttackType.Ranged:
                        bonus += GetShellArmorEffectValue(ShellOptionType.PvpDodgeRanged, true);
                        break;

                    case AttackType.Magical:
                        magicBonus += GetShellArmorEffectValue(ShellOptionType.PvpDodgeMagic, true);
                        break;
                }

                bonus += GetShellArmorEffectValue(ShellOptionType.SPvpDodgeAll, true);
            }

            if (attacker.AttackType != AttackType.Magical)
            {
                if (!attacker.HasBuff(BCardType.CardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AttackHitChance))
                {
                    if (ServerManager.Instance.RandomNumber() - bonus < chance)
                    {
                        hitmode = 1;
                        attacker.SkillBcards.Clear();
                        targetEntity.DealtDamage = 0;
                        return 0;
                    }
                }
                else
                {
                    chance = attacker.GetBuff(BCardType.CardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AttackHitChance, false)[0];

                    if (ServerManager.Instance.RandomNumber() > chance)
                    {
                        hitmode = 1;
                        attacker.SkillBcards.Clear();
                        targetEntity.DealtDamage = 0;
                        return 0;
                    }
                }
            }
            else
            {
                int magicalEvasiveness = (int)((1 - (1 - bonus / 100) * (1 - magicBonus / 100)) * 100);
                if (ServerManager.Instance.RandomNumber() < magicalEvasiveness)
                {
                    hitmode = 1;
                    attacker.SkillBcards.Clear();
                    targetEntity.DealtDamage = 0;
                    return 0;
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.Instance.RandomNumber(attacker.MinDamage, attacker.MaxDamage + 1);
            int weaponDamage =
                ServerManager.Instance.RandomNumber(attacker.WeaponDamageMinimum, attacker.WeaponDamageMaximum + 1);

            #region Attack Level Calculation

            int[] atklvlfix = GetDefenderBenefitingBuffs(BCardType.CardType.CalculatingLevel,
                (byte)AdditionalTypes.CalculatingLevel.CalculatedAttackLevel);

            if (atklvlfix[3] != 0)
            {
                attackUpgrade = (short)atklvlfix[0];
            }

            attackUpgrade -= (short)targetDefenseUpgrade;

            if (attackUpgrade < -10)
            {
                attackUpgrade = -10;
            }
            else if (attackUpgrade > 10)
            {
                attackUpgrade = 10;
            }

            switch (attackUpgrade)
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
            }

            #endregion

            baseDamage = (int)((int)((baseDamage + staticBoostCategory3 + weaponDamage + 15) * boostCategory3)
                                * shellBoostCategory3);

            #endregion

            #region Defense

            switch (attackUpgrade)
            {
                //default:
                //    if (attackUpgrade < 0)
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
                    && attacker.AttackType.Equals(AttackType.Close))
                || (GetAttackerBenefitingBuffs(BCardType.CardType.SpecialDefence,
                        (byte)AdditionalTypes.SpecialDefence.RangedDefenceNullified)[3] != 0
                    && attacker.AttackType.Equals(AttackType.Ranged))
                || (GetAttackerBenefitingBuffs(BCardType.CardType.SpecialDefence,
                        (byte)AdditionalTypes.SpecialDefence.MagicDefenceNullified)[3] != 0
                    && attacker.AttackType.Equals(AttackType.Magical)))
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

            attacker.CriticalChance += GetShellWeaponEffectValue(ShellOptionType.IncreaseCritChance);
            attacker.CriticalChance -= GetShellArmorEffectValue(ShellOptionType.ReduceCriticalChance);
            attacker.CriticalChance += attacker.GetBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.InflictingIncreased, false)[0];
            attacker.CriticalChance -= attacker.GetBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.InflictingReduced, false)[0];
            attacker.CriticalRate += GetShellWeaponEffectValue(ShellOptionType.IncreaseCritDamages);
            attacker.CriticalRate += attacker.GetBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreased, false)[0];
            attacker.CriticalRate -= target.GetBuff(BCardType.CardType.StealBuff, (byte)AdditionalTypes.StealBuff.ReduceCriticalReceivedChance, false)[0];

            if (target.CellonOptions != null)
            {
                attacker.CriticalRate -= target.CellonOptions.Where(s => s.Type == (byte)CellonType.CriticalDamageDecrease)
                    .Sum(s => s.Value);
            }

            if (target.HasBuff(BCardType.CardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.ReceivingChancePercent))
            {
                attacker.CriticalChance = target.GetBuff(BCardType.CardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.ReceivingChancePercent, false)[0];
            }

            if (target.HasBuff(BCardType.CardType.SniperAttack, (byte)AdditionalTypes.SniperAttack.ReceiveCriticalFromSniper) && skill.SkillVNum == 1124) // Hardcoded, but this has to be this way
            {
                attacker.CriticalChance = target.GetBuff(BCardType.CardType.SniperAttack, (byte)AdditionalTypes.SniperAttack.ReceiveCriticalFromSniper, false)[0];
            }

            if (target.HasBuff(BCardType.CardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.AlwaysReceives))
            {
                attacker.CriticalChance = 100;
            }

            if (target.HasBuff(BCardType.CardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.NeverReceives))
            {
                attacker.CriticalChance = 0;
            }

            if (ServerManager.Instance.RandomNumber() < attacker.CriticalChance && attacker.AttackType != AttackType.Magical)
            {
                double multiplier = attacker.CriticalRate / 100D;
                int reducer = 1 - target.GetBuff(BCardType.CardType.StealBuff, (byte)AdditionalTypes.StealBuff.ReduceCriticalReceivedChance, false)[0] / 100;
                double reducerChance = target.GetBuff(BCardType.CardType.StealBuff, (byte)AdditionalTypes.StealBuff.ReduceCriticalReceivedChance, false)[1];
                if (multiplier > 3)
                {
                    multiplier = 3;
                }

                normalDamage += (int)(normalDamage * multiplier);
                if (ServerManager.Instance.RandomNumber() < reducerChance)
                {
                    normalDamage *= reducer;
                }
                hitmode = 3;
            }

            #endregion

            #region Fairy Damage

            int fairyDamage = (int)((baseDamage + 100) * (attacker.ElementRate + attacker.ElementRateSp) / 100D);

            #endregion

            #region Elemental Damage Advantage

            double elementalBoost = 0;

            switch (attacker.Element)
            {
                case 0:
                    break;

                case 1:
                    target.Resistance = targetFireResistance;
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
                    target.Resistance = targetWaterResistance;
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
                    target.Resistance = targetLightResistance;
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
                    target.Resistance = targetDarkResistance;
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

            if (skill?.Element == 0 || (skill?.Element != attacker.Element && attacker.EntityType == EntityType.Player))
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
                (int)((int)((normalDamage + elementalDamage + morale + staticBoostCategory1)
                              * boostCategory1) * shellBoostCategory1);


            if (attacker.Entity is Character && targetEntity is Character)
            {
                totalDamage /= 2;
                totalDamage *= 1 + attacker.GetBuff(BCardType.CardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.AttackIncreasedInPVP, false)[0] / 100;
            }

            if (target.EntityType == EntityType.Monster || target.EntityType == EntityType.NPC)
            {
                totalDamage -= GetMonsterDamageBonus(target.Level);
            }

            if (totalDamage < 5)
            {
                totalDamage = ServerManager.Instance.RandomNumber(1, 6);
            }

            if (attacker.EntityType == EntityType.Monster || attacker.EntityType == EntityType.NPC)
            {
                totalDamage += GetMonsterDamageBonus(attacker.Level);
            }

            #endregion

            #region Onyx Wings

            int[] onyxBuff = GetAttackerBenefitingBuffs(BCardType.CardType.StealBuff,
                (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon);
            if (onyxBuff[0] > ServerManager.Instance.RandomNumber())
            {
                onyxEffect = true;
            }

            Buff.Buff critDefence = target.Buffs.FirstOrDefault(s =>
                s.Card.BCards.Any(c => c.Type == (byte)BCardType.CardType.VulcanoElementBuff && c.SubType == (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefence));

            if (hitmode == 3 && critDefence != null)
            {
                int? reduce = critDefence.Card.BCards.FirstOrDefault(c => c.Type == (byte)BCardType.CardType.VulcanoElementBuff && c.SubType == (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefence)
                    ?.FirstData;
                attacker.SkillBcards.Clear();
                targetEntity.DealtDamage = reduce ?? totalDamage;
                return reduce ?? totalDamage;
            }

            #endregion

            if (target.Buffs.Any(s => s.Card.CardId == 608) && target.Entity is Character chara) // This has no bcard, thx entwell, xoxo
            {
                chara.Session.SendPacket($"mslot {(attacker.Element == 0 ? 15 : 10 + attacker.Element)} 0");
                chara.RemoveBuff(608);
                chara.SkillComboCount++;
                chara.LastSkillCombo = DateTime.Now;
            }

            totalDamage += attacker.ChargeValue;
            attacker.ChargeValue = 0;

            if (target.HasBuff(BCardType.CardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.TransferAttackPower))
            {
                target.ChargeValue = totalDamage;
                attacker.SkillBcards.Clear();
                targetEntity.DealtDamage = 0;
                return 0;
            }

            if (target.HasBuff(BCardType.CardType.Block, (byte)AdditionalTypes.Block.ChanceAllDecreased))
            {
                if (ServerManager.Instance.RandomNumber() < target.GetBuff(BCardType.CardType.Block, (byte)AdditionalTypes.Block.ChanceAllDecreased)[0])
                {
                    totalDamage -= totalDamage * (target.GetBuff(BCardType.CardType.Block, (byte)AdditionalTypes.Block.ChanceAllDecreased)[1] / 100);
                }
            }

            if (target.HasBuff(BCardType.CardType.LightAndShadow, (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP))
            {
                // Need to hardcode this because entwell & opennos fucking sucks
                double reducer = 0;
                if (target.Buffs.All(s => s.Card.CardId != 618))
                {
                    reducer = (double)target.GetBuff(BCardType.CardType.LightAndShadow, (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP)[0] / 100;
                    totalDamage -= (ushort)(totalDamage * reducer);
                    if (target.Entity is Character manaReducer)
                    {
                        manaReducer.Mp -= (ushort)(totalDamage * reducer);
                        manaReducer.Session.SendPacket(manaReducer.GenerateStat());
                    }
                }
                else
                {
                    Card archimageRegen = ServerManager.Instance.GetCardByCardId(618);

                    if (archimageRegen != null && target.Entity is Character archimageCharacter)
                    {
                        reducer = ((double)archimageCharacter.Level / target.GetBuff(BCardType.CardType.LightAndShadow, (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP)[0] / 10);
                        totalDamage = (ushort)(reducer * totalDamage);
                        archimageCharacter.Mp -= (ushort)(totalDamage * reducer);
                        archimageCharacter.Session.SendPacket(archimageCharacter.GenerateStat());
                    }
                }
            }

            if (target.HasBuff(BCardType.CardType.VulcanoElementBuff, (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefence) && hitmode == 3)
            {
                totalDamage -= target.GetBuff(BCardType.CardType.VulcanoElementBuff, (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefence, false)[0];
                totalDamage = totalDamage <= 0 ? ServerManager.Instance.RandomNumber(3, 6) : totalDamage;
            }

            attacker.SkillBcards.Clear();
            targetEntity.DealtDamage = totalDamage;
            return totalDamage;
        }

        private static int GetMonsterDamageBonus(byte level)
        {
            if (level < 45)
            {
                return 0;
            }
            if (level < 55)
            {
                return level;
            }
            if (level < 60)
            {
                return level * 2;
            }
            if (level < 65)
            {
                return level * 3;
            }
            if (level < 70)
            {
                return level * 4;
            }
            return level * 5;
        }

        #endregion

        #region Singleton
        private static DamageHelper _instance;

        public static DamageHelper Instance => _instance ?? (_instance = new DamageHelper());
        #endregion
    }
}
