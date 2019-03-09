using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    public class CheckFaultDTO
    {
        public string ProductCalDID { get; set; }
        public decimal XAverage { get; set; }
        public decimal XLowLimit { get; set; }
        public decimal XUpperLimit { get; set; }
        public decimal XCenterLine { get; set; }
        public decimal RAverage { get; set; }
        public decimal RLowLimit { get; set; }
        public decimal RUpperLimit { get; set; }
        public decimal RCenterLine { get; set; }
        public decimal xValue { get; set; }
        public decimal rValue { get; set; }
    }
}