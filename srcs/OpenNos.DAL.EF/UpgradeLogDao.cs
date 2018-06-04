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
    public class UpgradeLogDao : MappingBaseDao<UpgradeLog, UpgradeLogDTO>, IUpgradeLogDao
    {
        public SaveResult InsertOrUpdate(ref UpgradeLogDTO upgrade)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long id = upgrade.Id;
                    UpgradeLog entity = context.UpgradeLog.FirstOrDefault(c => c.Id.Equals(id));

                    if (entity == null)
                    {
                        upgrade = Insert(upgrade, context);
                        return SaveResult.Inserted;
                    }

                    upgrade.Id = entity.Id;
                    upgrade = Update(entity, upgrade, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public UpgradeLogDTO Insert(UpgradeLogDTO upgrade, OpenNosContext context)
        {
            try
            {
                var entity = _mapper.Map<UpgradeLog>(upgrade);
                context.UpgradeLog.Add(entity);
                context.SaveChanges();
                return _mapper.Map<UpgradeLogDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public UpgradeLogDTO Update(UpgradeLog old, UpgradeLogDTO replace, OpenNosContext context)
        {
            if (old != null)
            {
                _mapper.Map(old, replace);
                context.SaveChanges();
            }
            return _mapper.Map<UpgradeLogDTO>(old);
        }
    }
}