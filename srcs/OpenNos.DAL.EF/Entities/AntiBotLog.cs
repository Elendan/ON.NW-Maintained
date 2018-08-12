using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.EF.Entities
{
    public class AntiBotLog
    {
        public long Id { get; set; }

        public long CharacterId { get; set; }

        public string CharacterName { get; set; }

        public bool Timeout { get; set; }

        public DateTime DateTime { get; set; }
    }
}