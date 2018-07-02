using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Helpers
{
    public class SkillHelper
    {
        #region Instantiation

        public SkillHelper()
        {
            NoDamageSkills = new List<short>(new short[] { 815, 848, 1120, 1123, 847, 916, 929, 1160, 818, 892, 907, 1061, 1095, 1174, 1176, 870, 872, 950, 952, 953, 1086, 1108, 1109, 1129, 1133, 1137, 1138, 1329 });
        }

        #endregion

        #region Properties

        public List<short> NoDamageSkills { get; set; }

        #endregion  


        #region Singleton

        private static SkillHelper _instance;

        public static SkillHelper Instance => _instance = (_instance ?? new SkillHelper());

        #endregion
    }
}
