using System;
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
    public class ExchangeLogDao : MappingBaseDao<ExchangeLog, ExchangeLogDTO>, IExchangeLogDao
    {
        public SaveResult InsertOrUpdate(ref ExchangeLogDTO exchange)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long id = exchange.Id;
                    ExchangeLog entity = context.ExchangeLog.FirstOrDefault(c => c.Id.Equals(id));

                    if (entity == null)
                    {
                        exchange = Insert(exchange, context);
                        return SaveResult.Inserted;
                    }

                    exchange.Id = entity.Id;
                    exchange = Update(entity, exchange, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public ExchangeLogDTO Insert(ExchangeLogDTO exchange, OpenNosContext context)
        {
            try
            {
                var entity = _mapper.Map<ExchangeLog>(exchange);
                context.ExchangeLog.Add(entity);
                context.SaveChanges();
                return _mapper.Map<ExchangeLogDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public ExchangeLogDTO Update(ExchangeLog old, ExchangeLogDTO replace, OpenNosContext context)
        {
            if (old != null)
            {
                _mapper.Map(old, replace);
                context.SaveChanges();
            }
            return _mapper.Map<ExchangeLogDTO>(old);
        }
    }
}