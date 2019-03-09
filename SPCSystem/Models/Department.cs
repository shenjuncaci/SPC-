using DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("基础部门表")]
    [PrimaryKey("DepartmentID")]
    public class Department
    {
        public string DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int SortNo { get; set; }
        public int IsCompany { get; set; }
        public string ParentID { get; set; }
        public string CompanyID { get; set; }
    }
}