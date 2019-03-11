using DataAccess;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("产品测量数据录入表")]
    [PrimaryKey("ProductDataID")]
    public class ProductCalData
    {
        public string ProductDataID { get; set; }
        public string ProductCalDID { get; set; }
        public string ProductCalID { get; set; }
        public double InputValue { get; set; }
    }
}