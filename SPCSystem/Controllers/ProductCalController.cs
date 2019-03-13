using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Repository;
using Utilities;
using System.Data.Common;
using System.Data;

namespace SPCSystem.Controllers
{
    [LoginAuthorize]
    public class ProductCalController : Controller
    {

        Repository<ProductCal> ProductCalre = new Repository<ProductCal>();
        Repository<ProductCalDetail> ProductCalDetailre = new Repository<ProductCalDetail>();
        Repository<ProductCalData> ProductCalDatare = new Repository<ProductCalData>();

        Repository<CalFault> FaultRe = new Repository<CalFault>();

        Repository<Product> Productre = new Repository<Product>();
        //
        // GET: /ProductCal/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                StringBuilder strSql = new StringBuilder();
                List<DbParameter> parameter = new List<DbParameter>();
                strSql.AppendFormat(@" select * from ProductCal where 1=1 and  DepartmentID='{0}'  ",ManageProvider.Provider.Current().DepartmentId);
                if (!CommonHelper.IsEmpty(keywords))
                {
                    strSql.AppendFormat(@" and (partname like '%{0}%' or productno like '%{0}%') ", keywords);
                }

                if (ManageProvider.Provider.Current().IsSystem == false)
                {
                    strSql.AppendFormat(@" and CompanyID='{0}' ", ManageProvider.Provider.Current().CompanyId);
                }
                if(ManageProvider.Provider.Current().IsCompanySystem==false)
                {
                    strSql.AppendFormat(@" and DepartmentID='{0}' ", ManageProvider.Provider.Current().DepartmentId);
                }

                DataTable ListData = ProductCalre.FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);

                var JsonData = new
                {
                    total = jqgridparam.total,
                    page = jqgridparam.page,
                    records = jqgridparam.records,
                    costtime = CommonHelper.TimerEnd(watch),
                    rows = ListData,
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult Form()
        {
            return View();
        }


        public ActionResult GridProductCalListJson(string keywords, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                StringBuilder strSql = new StringBuilder();
                List<DbParameter> parameter = new List<DbParameter>();
                strSql.AppendFormat(@" select a.PartName,a.ProductNO,a.Feature,a.Customer,a.PictureType,b.* from product a left join CalMethod b on a.ProductID=b.ProductID where 1=1    ");
                if (!CommonHelper.IsEmpty(keywords))
                {
                    strSql.AppendFormat(@" and (partname like '%{0}%' or productno like '%{0}%') ", keywords);
                }
                strSql.AppendFormat(@" and a.CompanyID='{0}' ", ManageProvider.Provider.Current().CompanyId);
                DataTable ListData = Productre.FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
                var JsonData = new
                {
                    total = jqgridparam.total,
                    page = jqgridparam.page,
                    records = jqgridparam.records,
                    costtime = CommonHelper.TimerEnd(watch),
                    rows = ListData,
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [HttpPost]
        public ActionResult InsertCalRecord(string CalID, string Eqiupment, string CalTool,string DataAcc)
        {
            try
            {
                StringBuilder strsql = new StringBuilder();
                strsql.AppendFormat(@" insert into ProductCal
select NEWID(),PartName,ProductNO,Feature,a.PictureType,a.Customer,b.CalID,
b.GroupNum,b.StartPPK,b.StandardLowLimit,b.StandardUpperLimit,b.StandardCenterLine,b.XLowLimit,b.XUpperLimit,b.XAverage,b.XCenterLine,
b.RAverage,b.RLowLimit,b.RUpperLimit,b.RCenterLine,b.Tolerance,'{0}',GETDATE(),'{1}',b.CalType,'{3}','{4}','{5}','{6}'
from product a left join CalMethod b on a.ProductID=b.ProductID where 1=1 and b.Calid='{2}'   "
                    , ManageProvider.Provider.Current().UserId, ManageProvider.Provider.Current().DepartmentId, CalID,ManageProvider.Provider.Current().CompanyId,
                    Eqiupment,CalTool,DataAcc);
                ProductCalre.ExecuteBySql(strsql);

                return Content(new JsonMessage { Success = true, Code = "1", Message = "添加成功" }.ToString());
            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }
        /// <summary>
        /// 判断每条记录的当前状态
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public string GetDataState(string ProductCalID)
        {
            string sql = " select GroupNum from dbo.ProductCal a where exists (select * from dbo.ProductCalDetail where State='进行中' and ProductCalID=a.ProductCalID) and  ProductCalID='{0}' ";
            sql = string.Format(sql, ProductCalID);
            DataTable dt = ProductCalre.FindTableBySql(sql);
            if(dt.Rows.Count>0)
            {
                //表示有正在进行中的检测，可以点击数据录入，结束检测，不能点击开始检测
                return "0";
            }
            else
            {
                //表示没有正在进行中的检测,可以点击开始检测按钮
                return "1";
            }
        }
        /// <summary>
        /// 开始一次测试
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public string InsertOneTest(string ProductCalID)
        {
            string sql = string.Format(" select * from dbo.ProductCalDetail where State='进行中' and ProductCalID='{0}' ",ProductCalID);
            DataTable dt = ProductCalre.FindTableBySql(sql);
            if(dt.Rows.Count>0)
            {
                //表示已经有在进行中的检测，当前状态不允许再新开一条检测
                return "0";
            }
            else
            {
                ProductCal maine = ProductCalre.FindEntity(ProductCalID);

                ProductCalDetail entity = new ProductCalDetail();
                entity.ProductCalDID = CommonHelper.GetGuid;
                entity.ProductCalID = ProductCalID;
                entity.StartDate = DateTime.Now;
                entity.State = "进行中";
                entity.UpdateMan = ManageProvider.Provider.Current().UserName;
                ProductCalDetailre.Insert(entity);
                //插入一条新的检测记录
                //循环插入5条空的检测录入记录,取消每次录入的按钮，改为一次性录入5条数据
                for(int i=0;i<maine.GroupNum;i++)
                {
                    ProductCalData entityData = new ProductCalData();
                    entityData.ProductDataID = CommonHelper.GetGuid;
                    entityData.ProductCalDID = entity.ProductCalDID;
                    entityData.ProductCalID = ProductCalID;
                    ProductCalDatare.Insert(entityData);
                }
                
                return "1";
            }
        }

        //数据录入的界面，比较简单，只需要一个input框
        public ActionResult DataInput()
        {
            return View();
        }

        
        /// <summary>
        /// 获取到的是主表ID，根据主表ID找到进行中的二级表ID，根据二级表的ID插入三级表
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult InsertInputData(string ProductCalID,string Num)
        {
            try
            {
                string sqldatacount = string.Format(@"select case when (select groupnum from ProductCal where ProductCalID='{0}')>
(select COUNT(1) from dbo.ProductCalData where ProductCalDID in (
select ProductCalDID from dbo.ProductCalDetail where State='进行中' and ProductCalID='{0}')) 
then 'success' else 'error' end ",ProductCalID);
                DataTable dtdatacount = ProductCalre.FindTableBySql(sqldatacount);
                if(dtdatacount.Rows[0][0].ToString()=="success") //表示成功，可以输入检测数据
                {
                    string sql = string.Format(" select ProductCalDID from dbo.ProductCalDetail where State='进行中' and ProductCalID='{0}' ",ProductCalID);
                    DataTable dt = ProductCalre.FindTableBySql(sql);
                    if(dt.Rows.Count>0)
                    {
                        string ProductCalDID = dt.Rows[0][0].ToString();
                        ProductCalData entity = new ProductCalData();
                        entity.ProductDataID = CommonHelper.GetGuid;
                        entity.ProductCalDID = ProductCalDID;
                        entity.ProductCalID = ProductCalID;
                        entity.InputValue = Convert.ToDouble(Num);
                        ProductCalDatare.Insert(entity);
                        return Content(new JsonMessage { Success = true, Code = "1", Message = "数据录入成功" }.ToString());
                    }
                    else
                    {
                        return Content(new JsonMessage { Success = true, Code = "1", Message = "异常，没有正在进行中的检测" }.ToString());
                    }

                    
                }
                else
                {
                    return Content(new JsonMessage { Success = true, Code = "1", Message = "本次的检测数据已经全部输入完成，无法再录入数据，请结束本次检测，重新开始下次检测" }.ToString());
                }

                
            }
            catch (Exception ex)
            { 
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        /// <summary>
        /// 结束本次检测，需要先检查录入的数据是否满足要求，满足的了才能录入
        /// 1.一点落在控制限以外					
        /// 2.连续9点落在中心线的同一侧								
        /// 3.连续6点递增或递减					
        /// 4.连续14点中相邻点交替上下
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <returns></returns>
        public string EndThisTest(string ProductCalID, string PictureType)
        {
            CalResultBll CalBll = new CalResultBll();
            MRCalResultBll MRBll = new MRCalResultBll();
            string sqldatacount = string.Format(@"select case when (select groupnum from ProductCal where ProductCalID='{0}')=
(select COUNT(1) from dbo.ProductCalData where InputValue is not null and ProductCalDID in (
select ProductCalDID from dbo.ProductCalDetail where State='进行中' and ProductCalID='{0}')) 
then 'success' else 'error' end ", ProductCalID);
            DataTable dtdatacount = ProductCalre.FindTableBySql(sqldatacount);
            if(dtdatacount.Rows[0][0].ToString()=="success") //表示成功，可以结束本次
            {
                StringBuilder strsql = new StringBuilder();
                strsql.AppendFormat(@" update dbo.ProductCalDetail set State='已完成',EndDate=GETDATE(),UpdateMan='{1}' where State='进行中' and ProductCalID='{0}' ", ProductCalID,ManageProvider.Provider.Current().UserName);
                ProductCalre.ExecuteBySql(strsql);
                //表示本次成功了.成功以后还需要做其他的分支判断，检测添加了本次以后是否有异常结果
                //CalBll.CheckFault(ProductCalID);
                if (PictureType == "X-Bar")
                {
                    return CalBll.CheckFault(ProductCalID);
                }
                else
                {
                    return MRBll.CheckFault(ProductCalID);
                }
               
            }
            else
            {
                //表示录入的数据量还不够结束本次的
                return "0";
            }
        }

        public ActionResult CheckInputData()
        {
            return View();
        }

        public ActionResult GridThisCheckListJson(string ProductCalID)
        {
            string sql = @" select * from ProductCalData where ProductCalDID in 
(select ProductCalDID from dbo.ProductCalDetail where ProductCalID='{0}' and State='进行中')  ";
            sql = string.Format(sql, ProductCalID);
            DataTable dt = ProductCalre.FindTableBySql(sql);
            //在前段判断是否有数据
            return Content(dt.ToJson());
        }

        //提交检测确认的数据
        [HttpPost]
        public ActionResult SubmitProductCalData(string ProductCalID,string Detail)
        {
            try
            {
                //第一步，先找到进行中的detailID
                string sql = "select ProductCalDID from ProductCalDetail where ProductCalID='{0}' and State='进行中'";
                sql = string.Format(sql, ProductCalID);
                DataTable dt = ProductCalDatare.FindTableBySql(sql);
                if(dt.Rows.Count<=0)
                {
                    return Content(new JsonMessage { Success = false, Code = "1", Message = "异常，没有进行中的检测记录，请刷新后重试" }.ToString());
                }
                string ProductCalDID = dt.Rows[0][0].ToString();

                List<ProductCalData> list = Detail.JonsToList<ProductCalData>();
                //删除原数据
                StringBuilder strsql = new StringBuilder();
                ProductCalDatare.Delete("ProductCalDID", ProductCalDID);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].ProductDataID = CommonHelper.GetGuid;
                    list[i].ProductCalID = ProductCalID;
                    list[i].ProductCalDID = ProductCalDID;
                    ProductCalDatare.Insert(list[i]);
                }
                return Content(new JsonMessage { Success = true, Code = "1", Message = "成功更新数据" }.ToString());
            }
            catch (Exception ex)
            {
                //database.Rollback();
                //database.Close();
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        public ActionResult SeeTrend()
        {
            return View();
        }

        /// <summary>
        /// 开始日期-结束日期
        /// </summary>
        /// <param name="ProductCalID"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public ActionResult GetTrend(string ProductCalID, string StartDate, string EndDate)
        {
            if (CommonHelper.IsEmpty(StartDate))
            {
                StartDate = DateTime.Now.AddDays(-30).ToString();
            }
            if (CommonHelper.IsEmpty(EndDate))
            {
                EndDate = DateTime.Now.ToString();
            }

            //decimal D1=2.114M;
            string sql = @"  select  CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8) as riqi,
AVG(InputValue) as xbar,MAX(InputValue)-MIN(InputValue) as r,
c.XLowLimit,c.XUpperLimit,c.XCenterLine,
c.RLowLimit,c.RUpperLimit,c.RCenterLine,
c.StandardLowLimit,c.StandardUpperLimit,c.StandardCenterLine
from ProductCalData a 
left join ProductCalDetail  b on a.ProductCalDID=b.ProductCalDID 
left join ProductCal c on b.ProductCalID=c.ProductCalID
where a.ProductCalID='{0}'  and cast(b.StartDate as date)>=cast('{1}' as date) and cast(b.StartDate as date)<=cast('{2}' as date) and  b.State='已完成'
group by a.ProductCalDID,b.StartDate,b.EndDate,c.XLowLimit,c.XUpperLimit,c.XCenterLine,c.RLowLimit,c.RUpperLimit,c.RCenterLine,
c.StandardLowLimit,c.StandardUpperLimit,c.StandardCenterLine 
order by StartDate ";
            sql = string.Format(sql, ProductCalID,StartDate,EndDate);
            DataTable dt = ProductCalDatare.FindTableBySql(sql);
            if(dt.Rows.Count>0)
            {
                //object rCenterLine = dt.Compute("Avg(" + "r" + ")", "true");
                //decimal rUCL = (decimal)rCenterLine * D1;
                //object xbarCenterLine = dt.Compute("Avg(" + "xbar" + ")", "true");
                //decimal xbarUCL = 1.8M;
                //decimal xbarLCL = -0.265M;

                //var JsonData = new
                //{
                //    Success = true,
                //    Code = "1",
                //    rCenterLine=rCenterLine,
                //    rUCL=rUCL,
                //    xbarCenterLine=xbarCenterLine,
                //    xbarUCL=xbarUCL,
                //    xbarLCL=xbarLCL,
                //    Data = dt,
                //};

                return Content(dt.ToJson());
            }
            else
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "该检测尚未录入数据，请先录入数据" }.ToString());
            }
            //return View();
        }

        public ActionResult GetMRTrend(string ProductCalID, string StartDate, string EndDate)
        {
            MRCalResultBll MrcalBll = new MRCalResultBll();
            if (CommonHelper.IsEmpty(StartDate))
            {
                StartDate = DateTime.Now.AddDays(-30).ToString();
            }
            if (CommonHelper.IsEmpty(EndDate))
            {
                EndDate = DateTime.Now.ToString();
            }

            //decimal D1=2.114M;
            string sql = @"  select  CONVERT(varchar(100), b.StartDate, 120)+'~'+CONVERT(varchar(100), b.EndDate, 8) as riqi,
CONVERT(varchar(100), b.StartDate, 120) as sdate,
CONVERT(varchar(100), b.EndDate, 8) as edate,
AVG(InputValue) as x,
c.XLowLimit,c.XUpperLimit,c.XCenterLine,
c.RLowLimit,c.RUpperLimit,c.RCenterLine,
c.StandardLowLimit,c.StandardUpperLimit,c.StandardCenterLine
from ProductCalData a 
left join ProductCalDetail  b on a.ProductCalDID=b.ProductCalDID 
left join ProductCal c on b.ProductCalID=c.ProductCalID
where a.ProductCalID='{0}'  and cast(b.StartDate as date)>=cast('{1}' as date) and cast(b.StartDate as date)<=cast('{2}' as date) and  b.State='已完成'
group by a.ProductCalDID,b.StartDate,b.EndDate,c.XLowLimit,c.XUpperLimit,c.XCenterLine,c.RLowLimit,c.RUpperLimit,c.RCenterLine,
c.StandardLowLimit,c.StandardUpperLimit,c.StandardCenterLine 
order by StartDate ";
            sql = string.Format(sql, ProductCalID, StartDate, EndDate);
            Repository<MRPictureDTO> re = new Repository<MRPictureDTO>();
            List<MRPictureDTO> list = re.FindListBySql(sql);
            list = MrcalBll.GetMRList(list);
            
            if(list.Count>0)
            {
                return Content(list.ToJson());
            }
            else
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "该检测尚未录入数据，请先录入数据" }.ToString());
            }
        }

        public string DetailTable(string ProductCalID, string StartDate, string EndDate)
        {
            return CalResultBll.DetailTable(ProductCalID, StartDate, EndDate, ProductCalre);
        }
        public string DefaultDetailTable(string ProductCalID)
        {
            return CalResultBll.DefaultDetailTable(ProductCalID, ProductCalre);
        }
        public string FaultTable(string ProductCalID,string StartDate,string EndDate)
        {
            return CalResultBll.FaultTTable(ProductCalID, StartDate, EndDate, ProductCalre);
        }
        public string PotentialRiskTable(string ProductCalID,string StartDate,string EndDate)
        {
            return CalResultBll.PotentialRisk(ProductCalID, StartDate, EndDate, ProductCalre);
        }
        public string CalCPK(string ProductCalID,string StartDate,string EndDate,string PictureType)
        {
            if(PictureType=="X_Bar")
            {
                return CalResultBll.CalCPK(ProductCalID, StartDate, EndDate, ProductCalre);
            }
            else
            {
                return MRCalResultBll.CalCPK(ProductCalID, StartDate, EndDate);
            }
            
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetForm(string ProductCalID)
        {
            ProductCal entity = ProductCalre.FindEntity(ProductCalID);
            if (entity == null)
            {
                return Content("");
            }
            string strJson = entity.ToJson();
            return Content(strJson);
        }

        public ActionResult FaultList()
        {
            return View();
        }

        public ActionResult GridFaultListJson(string keywords, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                StringBuilder strSql = new StringBuilder();
                List<DbParameter> parameter = new List<DbParameter>();
                strSql.AppendFormat(@" select a.*,b.PartName,ProductNO,Customer 
from CalFault a left join ProductCal b on a.ProductCalID=b.ProductCalID where 1=1   ");
                if(!ManageProvider.Provider.Current().IsSystem)
                {
                    strSql.AppendFormat(" and a.ProductCalID in (select ProductCalID from ProductCal where CompanyID='{0}') ",ManageProvider.Provider.Current().CompanyId);
                }
                if (!CommonHelper.IsEmpty(keywords))
                {
                    strSql.AppendFormat(@" and (b.PartName like '%{0}%' or b.ProductNO like '%{0}%' or b.Customer like '%{0}%') ", keywords);
                }
                //strSql.AppendFormat(@" and a.departmentid='{0}' ", ManageProvider.Provider.Current().DepartmentId);

                DataTable ListData = Productre.FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);

                var JsonData = new
                {
                    total = jqgridparam.total,
                    page = jqgridparam.page,
                    records = jqgridparam.records,
                    costtime = CommonHelper.TimerEnd(watch),
                    rows = ListData,
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                //Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult FaultForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetFaultForm(string KeyValue)
        {
            CalFault entity = FaultRe.FindEntity(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            //strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        [HttpPost]
        public ActionResult SubmitFaultForm(string KeyValue, CalFault entity)
        {
            try
            {
                CalFault entitySource = FaultRe.FindEntity(KeyValue);
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                entitySource.CalFaultID = KeyValue;
                entitySource.CauseAnaly = entity.CauseAnaly;
                entitySource.ImproveMeasure = entity.ImproveMeasure;
                FaultRe.Update(entitySource);


                return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
            }
            catch (Exception ex)
            {
                //database.Rollback();
                //database.Close();
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

    }
}