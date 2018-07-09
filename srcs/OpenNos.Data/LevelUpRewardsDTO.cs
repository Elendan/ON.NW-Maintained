
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using NosSharp.Enums;
using OpenNos.Data.Base;

namespace OpenNos.Data
{
    public class LevelUpRewardsDTO : MappingBaseDTO
    {
        [Key]
        public long Id { get; set; }

        public short Level { get; set; }

        public short JobLevel { get; set; }

        public short HeroLvl { get; set; }

        public short Vnum { get; set; }

        public short Amount { get; set; }

        public bool IsMate { get; set; }

        public short MateLevel { get; set; }

        public MateType MateType { get; set; }
    }
}