using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosSharp.Enums;
using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface ILevelUpRewardsDAO : IMappingBaseDAO
    {
        IEnumerable<LevelUpRewardsDTO> LoadByLevelAndLevelType(byte level, LevelType type);

        IEnumerable<LevelUpRewardsDTO> LoadByLevel(byte? level);

        IEnumerable<LevelUpRewardsDTO> LoadByJobLevel(byte? level);

        IEnumerable<LevelUpRewardsDTO> LoadByHeroLevel(byte? level);
    }
}
