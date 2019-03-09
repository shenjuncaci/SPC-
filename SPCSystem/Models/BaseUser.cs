using DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("基础用户表")]
    [PrimaryKey("UserID")]
    public class BaseUser
    {
        public string UserID { get; set; }
        public string Code { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DepartmentID { get; set; }
        public string CompanyID { get; set; }
        public int IsSystem { get; set; }
        public int IsAlarm { get; set; }
        public string AlarmLevel { get; set; }
        public string AlarmMail { get; set; }
    }
}