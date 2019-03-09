using DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("判异准则表")]
    [PrimaryKey("CriterionID")]
    public class Criterion
    {
        public string CriterionID { get; set; }
        public string CriterionNo { get; set; }
        public string CriterionName { get; set; }
       
    }
}