using DataAccess;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;

namespace SPCSystem
{
    [Description("产品基础数据表")]
    [PrimaryKey("ProductID")]
    public class Product
    {
        public string ProductID { get; set; }
        public string PartName { get; set; }
        public string ProductNO { get; set; }
        public string Feature { get; set; }
        public string PictureType { get; set; }
        public string Customer { get; set; }
        public string CompanyID { get; set; }
    }
}