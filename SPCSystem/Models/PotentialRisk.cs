using DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("潜在风险")]
    [PrimaryKey("PotentialID")]
    public class PotentialRisk
    {
        public string PotentialID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PotentialContent { get; set; }
        public string FailureEffect { get; set; }
        public string Action { get; set; }
        public string DepartmentID { get; set; }
        public string CompanyID { get; set; }
    }
}