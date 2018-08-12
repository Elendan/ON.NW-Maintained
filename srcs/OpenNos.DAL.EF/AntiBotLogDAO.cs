using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class AntiBotLogDAO : MappingBaseDao<AntiBotLog, AntiBotLogDTO>, IAntiBotLogDAO
    {
        public SaveResult InsertOrUpdate(ref AntiBotLogDTO log)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long logId = log.Id;
                    AntiBotLog entity = context.AntiBotLog.FirstOrDefault(c => c.Id.Equals(logId));

                    if (entity == null)
                    {
                        log = Insert(log, context);
                        return SaveResult.Inserted;
                    }

                    log.Id = entity.Id;
                    log = Update(entity, log, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        private AntiBotLogDTO Insert(AntiBotLogDTO log, OpenNosContext context)
        {
            try
            {
                var entity = _mapper.Map<AntiBotLog>(log);
                context.AntiBotLog.Add(entity);
                context.SaveChanges();
                return _mapper.Map<AntiBotLogDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private AntiBotLogDTO Update(AntiBotLog entity, AntiBotLogDTO respawn, OpenNosContext context)
        {
            if (entity == null)
            {
                return null;
            }

            _mapper.Map(respawn, entity);
            context.SaveChanges();
            return _mapper.Map<AntiBotLogDTO>(entity);
        }
    }
}