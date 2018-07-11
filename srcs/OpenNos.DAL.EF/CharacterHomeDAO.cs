// NosSharp
// CharacterHomeDAO.cs

using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Entities;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class CharacterHomeDAO : SynchronizableBaseDAO<CharacterHome, CharacterHomeDTO>, ICharacterHomeDAO
    {
        public IEnumerable<CharacterHomeDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (CharacterHome home in context.CharacterHome)
                {
                    yield return _mapper.Map<CharacterHomeDTO>(home);
                }
            }
        }

        public DeleteResult DeleteByName(long characterId, string name)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    CharacterHome entity = context.CharacterHome.FirstOrDefault(s => s.CharacterId == characterId && s.Name == name);
                    if (entity != null)
                    {
                        context.CharacterHome.Remove(entity);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(e.ToString());
                return DeleteResult.Error;
            }
        }

        public IEnumerable<CharacterHomeDTO> LoadByCharacterId(long id)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (CharacterHome home in context.CharacterHome.Where(s => s.CharacterId == id))
                {
                    yield return _mapper.Map<CharacterHomeDTO>(home);
                }
            }
        }

        public SaveResult InsertOrUpdate(ref CharacterHomeDTO dto)
        {
            var context = new OpenNosContext();
            SaveResult tmp = InsertOrUpdate(ref dto, ref context);
            context.SaveChanges();
            return tmp;
        }

        public SaveResult InsertOrUpdate(ref CharacterHomeDTO dto, ref OpenNosContext context)
        {
            try
            {
                Guid homeDtoId = dto.Id;
                CharacterHome entity = context.CharacterHome.FirstOrDefault(c => c.Id.Equals(homeDtoId));

                if (entity == null)
                {
                    dto = Insert(dto, context);
                    return SaveResult.Inserted;
                }

                dto = Update(entity, dto, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                return SaveResult.Error;
            }
        }
    }
}