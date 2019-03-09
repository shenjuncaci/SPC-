using DataAccess;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("产品测量方法表")]
    [PrimaryKey("CalID")]
    public class CalMethod
    {
        public string CalID { get; set; }
        public string ProductID { get; set; }
        public string CalType { get; set; }
        public int GroupNum { get; set; }
        public decimal StartPPK { get; set; }
        public decimal StandardLowLimit { get; set; }
        public decimal StandardUpperLimit { get; set; }
        public decimal StandardCenterLine { get; set; }
        public decimal XAverage { get; set; }
        public decimal XLowLimit { get; set; }
        public decimal XUpperLimit { get; set; }
        public decimal XCenterLine { get; set; }
        public decimal RAverage { get; set; }
        public decimal RLowLimit { get; set; }
        public decimal RUpperLimit { get; set; }
        public decimal RCenterLine { get; set; }
        public string Tolerance { get; set; }
        //public DateTime? test { get; set; }

    }
}