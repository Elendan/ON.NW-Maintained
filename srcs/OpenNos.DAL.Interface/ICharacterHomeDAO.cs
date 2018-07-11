using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface ICharacterHomeDAO : IMappingBaseDAO
    {
        SaveResult InsertOrUpdate(ref CharacterHomeDTO dto);

        IEnumerable<CharacterHomeDTO> LoadByCharacterId(long id);

        IEnumerable<CharacterHomeDTO> LoadAll(); 

        DeleteResult DeleteByName(long characterId, string name);
    }
}
