using System.Collections.Generic;
using OpenNos.Data;

namespace ON.NW.Customisation.NewCharCustomisation
{
    public class WrestlerBaseSkill
    {
        public WrestlerBaseSkill() => Skills = new List<CharacterSkillDTO>
        {
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1525 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1526 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1527 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1528 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1529 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1530 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1531 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1532 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1533 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1534 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1535 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1536 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1537 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1538 },
            new CharacterSkillDTO { CharacterId = 0, SkillVNum = 1539 }
        };

        public IEnumerable<CharacterSkillDTO> Skills { get; set; }
    }
}
