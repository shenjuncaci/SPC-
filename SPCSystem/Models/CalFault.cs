using DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("异常记录表")]
    [PrimaryKey("CalFaultID")]
    public class CalFault
    {
        public string CalFaultID { get; set; }
        public string ProductCalID { get; set; }
        public string ProductCalDID { get; set; }
        public string FaultContent { get; set; }
        public string FaultFrom { get; set; }
        public string CauseAnaly { get; set; }
        public string ImproveMeasure { get; set; }
        public string CreatrBy { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}