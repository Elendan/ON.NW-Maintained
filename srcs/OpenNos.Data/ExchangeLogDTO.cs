using System;
using OpenNos.Data.Base;

namespace OpenNos.Data
{
    public class ExchangeLogDTO : MappingBaseDTO
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        public long CharacterId { get; set; }

        public string CharacterName { get; set; }

        public long TargetAccountId { get; set; }

        public long TargetCharacterId { get; set; }

        public string TargetCharacterName { get; set; }

        public short ItemVnum { get; set; }

        public short ItemAmount { get; set; }

        public long Gold { get; set; }

        public DateTime Date { get; set; }
    }
}
