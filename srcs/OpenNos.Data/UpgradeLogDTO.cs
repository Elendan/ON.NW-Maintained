using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data.Base;

namespace OpenNos.Data
{
    public class UpgradeLogDTO : MappingBaseDTO
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        public long CharacterId { get; set; }

        public string CharacterName { get; set; }

        public string UpgradeType { get; set; }

        public bool? HasAmulet { get; set; }

        public DateTime Date { get; set; }

        public bool Success { get; set; }

        public short ItemVnum { get; set; }

        public string ItemName { get; set; }
    }
}
