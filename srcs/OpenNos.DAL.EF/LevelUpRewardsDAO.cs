using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosSharp.Enums;
using OpenNos.Data;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Entities;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class LevelUpRewardsDAO : MappingBaseDao<LevelUpRewards, LevelUpRewardsDTO>, ILevelUpRewardsDAO
    {
        public IEnumerable<LevelUpRewardsDTO> LoadByLevel(byte? level)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (LevelUpRewards reward in context.LevelUpRewards.Where(s => s.Level == level))
                {
                    yield return _mapper.Map<LevelUpRewardsDTO>(reward);
                }
            }
        }

        public IEnumerable<LevelUpRewardsDTO> LoadByJobLevel(byte? level)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (LevelUpRewards reward in context.LevelUpRewards.Where(s => s.JobLevel == level))
                {
                    yield return _mapper.Map<LevelUpRewardsDTO>(reward);
                }
            }
        }

        public IEnumerable<LevelUpRewardsDTO> LoadByHeroLevel(byte? level)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (LevelUpRewards reward in context.LevelUpRewards.Where(s => s.HeroLvl == level))
                {
                    yield return _mapper.Map<LevelUpRewardsDTO>(reward);
                }
            }
        }

        public IEnumerable<LevelUpRewardsDTO> LoadByLevelAndLevelType(byte level, LevelType type)
        {
            switch (type)
            {
                case LevelType.JobLevel:
                    return LoadByJobLevel(level);
                case LevelType.Heroic:
                    return LoadByHeroLevel(level);
                default:
                    return LoadByLevel(level);
            }
        }
    }
}
