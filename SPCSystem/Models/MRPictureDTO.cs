using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    public class MRPictureDTO
    {
        public string ProductCalDID { get; set; }
        public string riqi { get; set; }
        public string sdate { get; set; }
        public string edate { get; set; }
        public decimal x { get; set; }
        public decimal mr { get; set; }
        public decimal XLowLimit{get;set;}
        public decimal XUpperLimit{get;set;}
        public decimal XCenterLine{get;set;}
        public decimal RLowLimit{get;set;}
        public decimal RUpperLimit{get;set;}
        public decimal RCenterLine{get;set;}
        public decimal StandardLowLimit{get;set;}
        public decimal StandardUpperLimit{get;set;}
        public decimal StandardCenterLine{get;set;}
    }
}