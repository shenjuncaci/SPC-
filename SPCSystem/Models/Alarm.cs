using DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("报警设置表")]
    [PrimaryKey("AlarmID")]
    public class Alarm
    {
        public string AlarmID { get; set; }
        public string AlarmName { get; set; }
        public int AlarmTime { get; set; }
        public string CompanyID { get; set; }
    }
}