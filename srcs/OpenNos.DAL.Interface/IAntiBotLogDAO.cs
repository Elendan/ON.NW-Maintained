using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface IAntiBotLogDAO : IMappingBaseDAO
    {
        SaveResult InsertOrUpdate(ref AntiBotLogDTO botlog);
    }
}