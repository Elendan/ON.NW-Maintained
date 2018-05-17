using System;
using System.Collections.Generic;
using System.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Handling;
using OpenNos.Data;
using OpenNos.GameObject;
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
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("NEED_PARTNER_TEAM", 1));
                return;
            }

            if (partnerInTeam.SpInstance == null)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("NO_PARTNER_SP", 1));
                return;
            }

            if (partnerInTeam.SpInstance.PartnerSkill1 != 0 && psopPacket.SkillSlot == 0 ||
                partnerInTeam.SpInstance.PartnerSkill2 != 0 && psopPacket.SkillSlot == 1 ||
                partnerInTeam.SpInstance.PartnerSkill3 != 0 && psopPacket.SkillSlot == 2)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("ALREADY_HAVE_SKILL", 1));
                return;
            }

            if (partnerInTeam.IsUsingSp)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("REMOVE_PARTNER_SP", 1));
                return;
            }

            if (partnerInTeam.SpInstance.Agility < 100 && Session.Account.Authority < AuthorityType.GameMaster)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("NOT_ENOUGH_AGILITY", 1));
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

                Session.SendPacket(partnerInTeam.GenerateScPacket());
                Session.SendPacket(partnerInTeam.GeneratePski());
                partnerInTeam.SpInstance.Agility = 0;
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal("COMPETENCE_MASTERED", 1));
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
                        MapMonster target = Session?.CurrentMapInstance?.GetMonster(upetPacket.TargetId);
                        AttackMonster(attacker, mateSkill, target);
                    }

                    return;

                case UserType.Npc:
                    if (attacker.Hp > 0)
                    {
                        UseSkill(attacker, mateSkill, upetPacket.TargetId);
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
                switch (suctlPacket.TargetType)
                {
                    case UserType.Monster:
                        if (attacker.Hp > 0)
                        {
                            MapMonster target = Session?.CurrentMapInstance?.GetMonster(suctlPacket.TargetId);
                            AttackMonster(attacker, ski, target);
                        }

                        return;
                }
            }
        }

        public void UseSkill(Mate attacker, NpcMonsterSkill skill, long id)
        {
            if (attacker == null)
            {
                return;
            }
            if (skill == null)
            {
                skill = new NpcMonsterSkill
                {
                    SkillVNum = attacker.Monster.BasicSkill
                };
            }

            string st = $"su 2 {attacker.MateTransportId} 1 {attacker.MateTransportId} {skill.Skill.SkillVNum} {skill.Skill.Cooldown} {skill.Skill.AttackAnimation} {skill.Skill.Effect} {attacker.PositionX} {attacker.PositionY} 1 {(int)((double)attacker.Hp / attacker.HpLoad() * 100)} 0 -2 {skill.Skill.SkillType - 1}";
            attacker.LastSkillUse = DateTime.Now;
            attacker.Mp -= skill.Skill == null ? 0 : skill.Skill.MpCost;
            if (skill?.Skill?.Effect != null)
            {
                attacker.MapInstance.Broadcast(attacker.GenerateEff((int)skill?.Skill?.Effect));
            }
            Session.CurrentMapInstance?.Broadcast($"ct 2 {attacker.MateTransportId} 2 {id} {skill.Skill?.CastAnimation} {skill.Skill?.CastEffect} {skill.Skill?.SkillVNum}");
            Session.CurrentMapInstance?.Broadcast(st);
        }

        public void AttackMonster(Mate attacker, NpcMonsterSkill skill, MapMonster target)
        {
            if (target == null || attacker == null || !target.IsAlive || skill?.Skill?.MpCost > attacker.Mp)
            {
                return;
            }
            if (skill == null)
            {
                skill = new NpcMonsterSkill
                {
                    SkillVNum = attacker.Monster.BasicSkill
                };
            }
            attacker.LastSkillUse = DateTime.Now;
            attacker.Mp -= skill.Skill == null ? 0 : skill.Skill.MpCost;
            target.Monster.BCards.Where(s => s.CastType == 1).ToList().ForEach(s => s.ApplyBCards(attacker));
            Session.CurrentMapInstance?.Broadcast($"ct 2 {attacker.MateTransportId} 3 {target.MapMonsterId} {skill.Skill?.CastAnimation} {skill.Skill?.CastEffect} {skill.Skill?.SkillVNum}");
            attacker.BattleEntity.TargetHit(target, TargetHitType.SingleTargetHit, skill.Skill);
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
                    Session.Character.RemoveBuff(3000);
                    Session.Character.RemoveBuff(3001);
                    Session.Character.RemoveBuff(3002);
                    Session.Character.RemoveBuff(3003);
                    Session.Character.RemoveBuff(3004);
                    Session.Character.RemoveBuff(3005);
                    Session.Character.RemoveBuff(3006);
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

                int sum = (mate.SpInstance.SkillRank1 + mate.SpInstance.SkillRank1 + mate.SpInstance.SkillRank1) / 3;
                Session.Character.AddBuff(new Buff(3000 + (sum - 1), isPermaBuff: true));
            }
        }
    }
}