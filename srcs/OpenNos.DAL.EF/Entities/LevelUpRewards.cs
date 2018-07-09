using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.EF.Entities
{
    public class LevelUpRewards
    {
        public long Id { get; set; }

        public short? Level { get; set; }

        public short? JobLevel { get; set; }

        public short? HeroLvl { get; set; }

        public short Vnum { get; set; }

        public short Amount { get; set; }

        public bool? IsMate { get; set; }
    }
}
