using DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("产品关联的判异准则表")]
    [PrimaryKey("CriterionID")]
    public class CalCriterion
    {
        public string CalCriterionID { get; set; }
        public string ProductID { get; set; }
        public string CriterionNo { get; set; }
        public string CriterionName { get; set; }

    }
}