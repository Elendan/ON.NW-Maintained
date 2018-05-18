using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Handling;
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.GameObject.Battle;
using OpenNos.GameObject.Buff;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Packets.ClientPackets;
using OpenNos.GameObject.Packets.ServerPackets;

namespace OpenNos.Handler
{
    public class MatePacketHandler : IPacketHandler
    {
        public MatePacketHandler(ClientSession session) => Session = session;

        private ClientSession Session { get; }

        /// <summary>
        ///     ps_op packet
        /// </summary>
        /// <param name="psopPacket"></param>
        public void LearnSkill(PsopPacket psopPacket)
        {
            Mate partnerInTeam = Session.Character.Mates.FirstOrDefault(s => s.IsTeamMember && s.MateType == MateType.Partner);
            if (partnerInTeam == null || psopPacket.PetId != partnerInTeam.PetId)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("Pas de partenaire dans l'équipe.", 1));
                return;
            }

            if (partnerInTeam.SpInstance == null)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("Le partenaire n'a pas de sp", 1));
                return;
            }

            if (partnerInTeam.SpInstance.PartnerSkill1 != 0 && psopPacket.SkillSlot == 0 ||
                partnerInTeam.SpInstance.PartnerSkill2 != 0 && psopPacket.SkillSlot == 1 ||
                partnerInTeam.SpInstance.PartnerSkill3 != 0 && psopPacket.SkillSlot == 2)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("Le partenaire possède déjà ce skill", 1));
                return;
            }

            if (partnerInTeam.IsUsingSp)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("Veuillez enlever la transformation de spécialiste", 1));
                return;
            }

            if (partnerInTeam.SpInstance.Agility < 100 && Session.Account.Authority < AuthorityType.GameMaster)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("Pas assez d'adresse", 1));
                return;
            }

            if (psopPacket.Option == 0)
            {
                Session.SendPacket($"delay 3000 12 #ps_op^{psopPacket.PetId}^{psopPacket.SkillSlot}^1");
                Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(2, 2, partnerInTeam.MateTransportId), partnerInTeam.PositionX, partnerInTeam.PositionY);
            }
            else
            {
                switch (psopPacket.SkillSlot)
                {
                    case 0:
                        partnerInTeam.SpInstance.PartnerSkill1 = MateHelper.Instance.PartnerSkills(partnerInTeam.SpInstance.Item.VNum, psopPacket.SkillSlot);
                        partnerInTeam.SpInstance.SkillRank1 = (byte)ServerManager.Instance.RandomNumber(1, 8);
                        break;
                    case 1:
                        partnerInTeam.SpInstance.PartnerSkill2 = MateHelper.Instance.PartnerSkills(partnerInTeam.SpInstance.Item.VNum, psopPacket.SkillSlot);
                        partnerInTeam.SpInstance.SkillRank2 = (byte)ServerManager.Instance.RandomNumber(1, 8);
                        break;
                    case 2:
                        partnerInTeam.SpInstance.PartnerSkill3 = MateHelper.Instance.PartnerSkills(partnerInTeam.SpInstance.Item.VNum, psopPacket.SkillSlot);
                        partnerInTeam.SpInstance.SkillRank3 = (byte)ServerManager.Instance.RandomNumber(1, 8);
                        break;
                }

                partnerInTeam.SpInstance.Agility = 0;
                Session.SendPacket(partnerInTeam.GenerateScPacket());
                Session.SendPacket(partnerInTeam.GeneratePski());
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("Ton partenaire maîtrise maintenant cette compétence", 1));
            }
        }

        /// <summary>
        ///     u_ps packet
        /// </summary>
        /// <param name="upsPacket"></param>
        public void SpecialPartnerSkill(UpsPacket upsPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }

                return;
            }

            Mate attacker = Session.Character.Mates.FirstOrDefault(x => x.MateTransportId == upsPacket.MateTransportId);
            if (attacker == null)
            {
                return;
            }
            Skill mateSkill = null;

            short? skillVnum = null;
            byte value = 0;
            switch (upsPacket.SkillSlot)
            {
                case 0:
                    skillVnum = attacker.SpInstance?.PartnerSkill1;
                    value = 0;
                    break;
                case 1:
                    skillVnum = attacker.SpInstance?.PartnerSkill2;
                    value = 1;
                    break;
                case 2:
                    skillVnum = attacker.SpInstance?.PartnerSkill3;
                    value = 2;
                    break;
            }

            if (skillVnum == null)
            {
                return;
            }

            mateSkill = ServerManager.Instance.GetSkill(skillVnum.Value);

            if (mateSkill == null)
            {
                return;
            }

            Observable.Timer(TimeSpan.FromSeconds(mateSkill.Cooldown * 0.1)).Subscribe(x =>
            {
                attacker.Owner?.Session.SendPacket($"psr {value}");
            });

            if (attacker.IsSitting)
            {
                return;
            }

            switch (upsPacket.TargetType)
            {
                case UserType.Monster:
                    if (attacker.Hp > 0)
                    {
                        MapMonster target = Session?.CurrentMapInstance?.GetMonster(upsPacket.TargetId);
                        AttackMonster(attacker, mateSkill, upsPacket.TargetId);
                    }

                    return;

                case UserType.Npc:
                    if (attacker.Hp > 0)
                    {
                        AttackMonster(attacker, mateSkill, upsPacket.TargetId);
                    }
                    return;

                case UserType.Player:
                    return;

                case UserType.Object:
                    return;

                default:
                    return;
            }
        }

        /// <summary>
        ///     u_pet packet
        /// </summary>
        /// <param name="upetPacket"></param>
        public void SpecialSkill(UpetPacket upetPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }

                return;
            }

            Mate attacker = Session.Character.Mates.FirstOrDefault(x => x.MateTransportId == upetPacket.MateTransportId);
            if (attacker == null)
            {
                return;
            }

            NpcMonsterSkill mateSkill = null;
            if (attacker.Monster.Skills.Any())
            {
                mateSkill = attacker.Monster.Skills.FirstOrDefault(x => x.Rate == 0);
            }

            if (mateSkill == null)
            {
                mateSkill = new NpcMonsterSkill
                {
                    SkillVNum = 200
                };
            }

            if (attacker.IsSitting)
            {
                return;
            }

            switch (upetPacket.TargetType)
            {
                case UserType.Monster:
                    if (attacker.Hp > 0)
                    {
                        AttackMonster(attacker, mateSkill, upetPacket.TargetId);
                    }

                    return;

                case UserType.Npc:
                    if (attacker.Hp > 0)
                    {
                        AttackMonster(attacker, mateSkill.Skill, upetPacket.TargetId);
                    }
                    return;

                case UserType.Player:
                    return;

                case UserType.Object:
                    return;

                default:
                    return;
            }
        }

        /// <summary>
        ///     suctl packet
        /// </summary>
        /// <param name="suctlPacket"></param>
        public void Attack(SuctlPacket suctlPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }

                return;
            }
            Mate attacker = Session.Character.Mates.FirstOrDefault(x => x.MateTransportId == suctlPacket.MateTransportId);
            if (attacker == null)
            {
                return;
            }

            if (attacker.IsSitting)
            {
                return;
            }

            IEnumerable<NpcMonsterSkill> mateSkills = attacker.IsUsingSp ? attacker.SpSkills.ToList() : attacker.Monster.Skills;
            if (mateSkills != null)
            {
                NpcMonsterSkill ski = mateSkills.FirstOrDefault(s => s?.Skill?.CastId == suctlPacket.CastId);
                if (ski == null)
                {
                    ski = new NpcMonsterSkill
                    {
                        SkillVNum = 200
                    };
                }
                switch (suctlPacket.TargetType)
                {
                    case UserType.Monster:
                        if (attacker.Hp > 0)
                        {
                            AttackMonster(attacker, ski, suctlPacket.TargetId);
                        }

                        return;
                }
            }
        }

        public void AttackMonster(Mate attacker, NpcMonsterSkill skill, long id)
        {
            AttackMonster(attacker, skill.Skill, id);
        }

        public void AttackMonster(Mate attacker, Skill skill, long targetId)
        {
            if (attacker == null || skill == null || skill?.MpCost > attacker.Mp)
            {
                return;
            }
            attacker.LastSkillUse = DateTime.Now;
            attacker.Mp -= skill.MpCost;
            if (skill.TargetType == 1 && skill.HitType == 1)
            {
                if (Session.HasCurrentMapInstance && skill.TargetRange != 0)
                {
                    //Probably some pvp stuff in here
                    foreach (MapMonster mon in attacker.MapInstance.GetListMonsterInRange(attacker.PositionX, attacker.PositionY, skill.TargetRange).Where(s => s.CurrentHp > 0))
                    {
                        attacker.BattleEntity.TargetHit(mon, TargetHitType.AOETargetHit, skill, skill.Effect);
                        Session.CurrentMapInstance?.Broadcast($"ct 2 {attacker.MateTransportId} 2 {mon.MapMonsterId} {skill?.CastAnimation} {skill?.CastEffect} {skill?.SkillVNum}");
                    }
                }
            }
            else if (skill.TargetType == 2 && skill.HitType == 0)
            {
                ClientSession target = attacker.Owner.Session ?? Session;
                Session.CurrentMapInstance?.Broadcast($"ct 2 {attacker.MateTransportId} 2 {targetId} {skill?.CastAnimation} {skill?.CastEffect} {skill?.SkillVNum}");
                skill.BCards.ToList().ForEach(s =>
                {
                    // Apply skill bcards to ower and pet 
                    s.ApplyBCards(target.Character);
                    s.ApplyBCards(attacker);
                });
            }
            else if (skill.TargetType == 1 && skill.HitType != 1)
            {
                Session.CurrentMapInstance?.Broadcast($"ct 2 {attacker.MateTransportId} 2 {attacker.MateTransportId} {skill?.CastAnimation} {skill?.CastEffect} {skill?.SkillVNum}");
                Session.CurrentMapInstance?.Broadcast($"su 2 {attacker.MateTransportId} 2 {attacker.MateTransportId} {skill.SkillVNum} {skill.Cooldown} {skill.AttackAnimation} {skill?.Effect} 0 0 1 100 0 -1 0");
                switch (skill.HitType)
                {
                    case 2:
                        IEnumerable<IBattleEntity> entityInRange = attacker.MapInstance?.GetBattleEntitiesInRange(attacker.GetPos(), skill.TargetRange)
                            .Where(b => b.SessionType() == SessionType.Character || b.SessionType() == SessionType.MateAndNpc);
                        if (entityInRange != null)
                        {
                            foreach (IBattleEntity target in entityInRange)
                            {
                                foreach (BCard s in skill.BCards)
                                {
                                    if (s.Type != (short)BCardType.CardType.Buff)
                                    {
                                        s.ApplyBCards(target, attacker);
                                        continue;
                                    }

                                    switch (attacker.MapInstance.MapInstanceType)
                                    {
                                        case MapInstanceType.Act4Instance:
                                            //later
                                            break;
                                        case MapInstanceType.ArenaInstance:
                                            // later
                                            break;
                                        default:
                                            s.ApplyBCards(target);
                                            break;
                                    }
                                }
                            }
                        }
                        break;
                    case 4:
                    case 0:
                        foreach (BCard bc in skill.BCards)
                        {
                            int bonusbuff = 0;

                            if (attacker.SpInstance?.PartnerSkill1 == skill.SkillVNum)
                            {
                                bonusbuff = (int)(attacker.SpInstance?.SkillRank1 - 1);
                            }
                            else if (attacker.SpInstance?.PartnerSkill2 == skill.SkillVNum)
                            {
                                bonusbuff = (int)(attacker.SpInstance?.SkillRank2 - 1);
                            }
                            else if (attacker.SpInstance?.PartnerSkill3 == skill.SkillVNum)
                            {
                                bonusbuff = (int)(attacker.SpInstance?.SkillRank3 - 1);
                            }

                            if (bc.Type == (short)BCardType.CardType.Buff)
                            {
                                bc.ApplyBCards(attacker, partnerBuffLevel: (short?)bonusbuff);
                                bc.ApplyBCards(attacker.Owner, partnerBuffLevel: (short?)bonusbuff);
                            }
                            else
                            {
                                bc.ApplyBCards(attacker);
                            }
                        }
                        break;
                }
            }
            else if (skill.TargetType == 0 && Session.HasCurrentMapInstance)
            {
                MapMonster monsterToAttack = attacker.MapInstance.GetMonster(targetId);
                if (monsterToAttack != null && attacker.Mp > skill.MpCost)
                {
                    if (Map.GetDistance(new MapCell { X = monsterToAttack.MapX, Y = monsterToAttack.MapY }, new MapCell { X = monsterToAttack.MapX, Y = monsterToAttack.MapY }) <
                        skill.Range + 1 + monsterToAttack.Monster.BasicArea)
                    {
                        foreach (BCard bc in skill.BCards)
                        {
                            var bf = new Buff(bc.SecondData);
                            if (bf.Card?.BuffType == BuffType.Bad || bf.Card?.BuffType == BuffType.Neutral)
                            {
                                bc.ApplyBCards(monsterToAttack, attacker);
                            }
                        }

                        Session.SendPacket(attacker.GenerateStatInfo());
                        Session.CurrentMapInstance?.Broadcast($"ct 2 {attacker.MateTransportId} 2 {attacker.MateTransportId} {skill?.CastAnimation} {skill?.CastEffect} {skill?.SkillVNum}");

                        if (skill.HitType == 3)
                        {
                            attacker.BattleEntity.TargetHit(monsterToAttack, TargetHitType.SingleAOETargetHit, skill, skill.Effect);
                        }
                        else
                        {
                            if (skill.TargetRange != 0)
                            {
                                IEnumerable<MapMonster> monstersInAorRange = attacker.MapInstance?.GetListMonsterInRange(monsterToAttack.MapX, monsterToAttack.MapY, skill.TargetRange)
                                    .Where(s => s.IsFactionTargettable(attacker.Owner.Faction));

                                attacker.BattleEntity.TargetHit(monsterToAttack, TargetHitType.SingleAOETargetHit, skill, skill.Effect);

                                if (monstersInAorRange != null)
                                {
                                    foreach (MapMonster mon in monstersInAorRange)
                                    {
                                        attacker.BattleEntity.TargetHit(mon, TargetHitType.SingleAOETargetHit, skill, skill.Effect);
                                    }
                                }
                            }
                            else
                            {
                                if (!monsterToAttack.IsAlive)
                                {
                                    Session.SendPacket("cancel 2 0");
                                    return;
                                }

                                attacker.BattleEntity.TargetHit(monsterToAttack, TargetHitType.SingleTargetHit, skill);
                            }
                        }
                    }
                }
            }
        }

        public void AttackCharacter(Mate attacker, NpcMonsterSkill skill, Character target)
        {
        }

        /// <summary>
        ///     psl packet
        /// </summary>
        /// <param name="pslPacket"></param>
        public void Psl(PslPacket pslPacket)
        {
            Mate mate = Session.Character.Mates.FirstOrDefault(x => x.IsTeamMember && x.MateType == MateType.Partner);
            if (mate == null)
            {
                return;
            }

            if (pslPacket.Type == 0)
            {
                if (mate.IsUsingSp)
                {
                    mate.IsUsingSp = false;
                    mate.SpSkills = null;
                    Session.Character.MapInstance.Broadcast(mate.GenerateCMode(-1));
                    Session.SendPacket(mate.GenerateCond());
                    Session.SendPacket(mate.GeneratePski());
                    Session.SendPacket(mate.GenerateScPacket());
                    Session.Character.MapInstance.Broadcast(mate.GenerateOut());
                    Session.Character.MapInstance.Broadcast(mate.GenerateIn());
                    Session.SendPacket(Session.Character.GeneratePinit());
                    Session.Character.RemoveBuff(3000, true);
                    Session.Character.RemoveBuff(3001, true);
                    Session.Character.RemoveBuff(3002, true);
                    Session.Character.RemoveBuff(3003, true);
                    Session.Character.RemoveBuff(3004, true);
                    Session.Character.RemoveBuff(3005, true);
                    Session.Character.RemoveBuff(3006, true);
                    //psd 30
                }
                else
                {
                    Session.SendPacket("delay 5000 3 #psl^1 ");
                    Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(2, 2, mate.MateTransportId), mate.PositionX, mate.PositionY);
                }
            }
            else
            {
                if (mate.SpInstance == null)
                {
                    return;
                }

                mate.IsUsingSp = true;
                //TODO: update pet skills
                mate.SpSkills = new NpcMonsterSkill[3];
                Session.SendPacket(mate.GenerateCond());
                Session.Character.MapInstance.Broadcast(mate.GenerateCMode(mate.SpInstance.Item.Morph));
                Session.SendPacket(mate.GeneratePski());
                Session.SendPacket(mate.GenerateScPacket());
                Session.Character.MapInstance.Broadcast(mate.GenerateOut());
                Session.Character.MapInstance.Broadcast(mate.GenerateIn());
                Session.SendPacket(Session.Character.GeneratePinit());
                Session.Character.MapInstance.Broadcast(mate.GenerateEff(196));
                //TODO: Fix this & find a link
                if (mate.SpInstance.Item.Morph != 2378)
                {
                    return;
                }

                int sum = (mate.SpInstance.SkillRank1 + mate.SpInstance.SkillRank2 + mate.SpInstance.SkillRank3) / 3;
                Session.Character.AddBuff(new Buff(3000 + (sum - 1), isPermaBuff: true));
            }
        }
    }
}