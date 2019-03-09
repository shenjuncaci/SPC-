using DataAccess;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("产品测量数据明细表")]
    [PrimaryKey("ProductCalDID")]
    public class ProductCalDetail
    {
        public string ProductCalDID { get; set; }
        public string ProductCalID { get; set; }
        public string State { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UpdateMan { get; set; }
    }
}