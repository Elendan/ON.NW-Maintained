using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data.Base;

namespace OpenNos.Data
{
    public class AntiBotLogDTO : MappingBaseDTO
    {
        [Key]
        public long Id { get; set; }

        public long CharacterId { get; set; }

        public string CharacterName { get; set; }

        public bool Timeout { get; set; }

        public DateTime DateTime { get; set; }
    }
}