using System.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Helpers
{
    public class RewardsHelper
    {
        #region Methods

        public int ArenaXpReward(byte characterLevel)
        {
            if (characterLevel <= 39)
            {
                // 25%
                return (int)(CharacterHelper.Instance.XpData[characterLevel] / 4);
            }

            if (characterLevel <= 55)
            {
                // 20%
                return (int)(CharacterHelper.Instance.XpData[characterLevel] / 5);
            }

            if (characterLevel <= 75)
            {
                // 10%
                return (int)(CharacterHelper.Instance.XpData[characterLevel] / 10);
            }

            if (characterLevel <= 79)
            {
                // 5%
                return (int)(CharacterHelper.Instance.XpData[characterLevel] / 20);
            }

            if (characterLevel <= 85)
            {
                // 2%
                return (int)(CharacterHelper.Instance.XpData[characterLevel] / 50);
            }

            if (characterLevel <= 90)
            {
                return (int)(CharacterHelper.Instance.XpData[characterLevel] / 80);
            }

            if (characterLevel <= 93)
            {
                return (int)(CharacterHelper.Instance.XpData[characterLevel] / 100);
            }

            if (characterLevel <= 99)
            {
                return (int)(CharacterHelper.Instance.XpData[characterLevel] / 1000);
            }

            return 0;
        }

        public void GetLevelUpRewards(ClientSession session)
        {
            if (session == null || !DaoFactory.LevelUpRewardsDao.LoadByLevel(session.Character.Level).Any())
            {
                return;
            }

            DaoFactory.LevelUpRewardsDao.LoadByLevel(session.Character.Level).ToList().ForEach(s =>
            {
                if (!s.IsMate)
                {
                    session.Character.GiftAdd(s.Vnum, (ushort)s.Amount);
                }
                else if (s.IsMate && s.MateLevel < session.Character.Level && s.MateLevel > 0)
                {
                    MateHelper.Instance.AddPetToTeam(session, s.Vnum, (byte)s.MateLevel, s.MateType);
                }
            });
        }

        public void GetJobRewards(ClientSession session)
        {
            if (session == null || !DaoFactory.LevelUpRewardsDao.LoadByJobLevel(session.Character.JobLevel).Any())
            {
                return;
            }

            DaoFactory.LevelUpRewardsDao.LoadByJobLevel(session.Character.JobLevel).ToList().ForEach(s =>
            {
                if (!s.IsMate)
                {
                    session.Character.GiftAdd(s.Vnum, (ushort)s.Amount);
                }
                else if (s.IsMate && s.MateLevel < session.Character.Level && s.MateLevel > 0)
                {
                    MateHelper.Instance.AddPetToTeam(session, s.Vnum, (byte)s.MateLevel, s.MateType);
                }
            });
        }

        public void GetHeroLvlRewards(ClientSession session)
        {
            if (session == null || !DaoFactory.LevelUpRewardsDao.LoadByHeroLevel(session.Character.HeroLevel).Any())
            {
                return;
            }

            DaoFactory.LevelUpRewardsDao.LoadByHeroLevel(session.Character.HeroLevel).ToList().ForEach(s =>
            {
                if (!s.IsMate)
                {
                    session.Character.GiftAdd(s.Vnum, (ushort) s.Amount);
                }
                else if (s.IsMate && s.MateLevel < session.Character.Level && s.MateLevel > 0)
                {
                    MateHelper.Instance.AddPetToTeam(session, s.Vnum, (byte)s.MateLevel, s.MateType);
                }
            });
        }

        #endregion

        #region Singleton

        private static RewardsHelper _instance;

        public static RewardsHelper Instance => _instance ?? (_instance = new RewardsHelper());

        #endregion
    }
}