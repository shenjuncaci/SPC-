using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.EnterpriseServices;
using DataAccess;

namespace SPCSystem
{
    [Description("数据录入主表")]
    [PrimaryKey("ProductCalID")]
    public class ProductCal
    {
        public string ProductCalID { get; set; }
        public string PartName { get; set; }
        public string ProductNO { get; set; }
        public string Feature { get; set; }
        public string PictureType { get; set; }
        public string Customer { get; set; }
        public string CalID { get; set; }
        public string CalType { get; set; }
        public int GroupNum { get; set; }
        public decimal StartPPK { get; set; }
        public decimal StandardLowLimit { get; set; }
        public decimal StandardUpperLimit { get; set; }
        public decimal StandardCenterLine { get; set; }
        public decimal XLowLimit { get; set; }
        public decimal XUpperLimit { get; set; }
        public decimal XAverage { get; set; }
        public decimal XCenterLine { get; set; }
        public decimal RAverage { get; set; }
        public decimal RLowLimit { get; set; }
        public decimal RUpperLimit { get; set; }
        public decimal RCenterLine { get; set; }
        public string Tolerance { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string DepartmentID { get; set; }
        public string CompanyID { get; set; }
        
    }
}