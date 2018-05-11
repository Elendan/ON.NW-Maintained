using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosSharp.Enums;

namespace OpenNos.GameObject.Battle
{
    public class MTListHitTarget
    {
        #region Instantiation
        public MTListHitTarget(UserType entityType, long targetId)
        {
            EntityType = entityType;
            TargetId = targetId;
        }

        #endregion


        #region Properties

        public UserType EntityType { get; set; }

        public long TargetId { get; set; }
        #endregion
    }
}
