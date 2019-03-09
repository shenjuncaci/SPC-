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
    public class DepartmentController : Controller
    {
        Repository<Department> DepartmentRe = new Repository<Department>();
        Repository<BaseUser> UserRe = new Repository<BaseUser>();
        DepartmentBll departmentbll = new DepartmentBll();
        //
        // GET: /Department/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TreeJson()
        {
            DataTable dt = departmentbll.GetTree(DepartmentRe);
            List<TreeJsonEntity> TreeList = new List<TreeJsonEntity>();
            if (!DataHelper.IsExistRows(dt))
            {
                foreach (DataRow row in dt.Rows)
                {
                    string DepartmentId = row["departmentid"].ToString();
                    bool hasChildren = false;
                    DataTable childnode = DataHelper.GetNewDataTable(dt, "parentid='" + DepartmentId + "'");
                    if (childnode.Rows.Count > 0)
                    {
                        hasChildren = true;
                    }
                    TreeJsonEntity tree = new TreeJsonEntity();
                    tree.id = DepartmentId;
                    tree.text = row["DepartmentName"].ToString();
                    tree.value = row["departmentid"].ToString();
                    tree.parentId = row["parentid"].ToString();
                    tree.isexpand = true;
                    tree.complete = true;
                    tree.hasChildren = hasChildren;
                    if (row["parentid"].ToString() == "0")
                    {
                        //tree.img = "/Content/Images/Icon16/molecule.png";
                        tree.img = "/Scripts/tree/images/icons/hostname.png";
                    }
                    else if(row["parentid"].ToString() == "-1")
                    {
                        tree.img = "/Scripts/tree/images/icons/molecule.png";
                    }
                    else
                    {
                        tree.img = "/Scripts/tree/images/icons/chart_organisation.png";
                    }
                    //else if (row["sort"].ToString() == "Company")
                    //{
                    //    tree.img = "/Content/Images/Icon16/hostname.png";
                    //}
                    //else if (row["sort"].ToString() == "Department")
                    //{
                    //    tree.img = "/Content/Images/Icon16/chart_organisation.png";
                    //}
                    TreeList.Add(tree);
                }
            }
            return Content(TreeList.TreeToJson());
        }

        public ActionResult GridPageListJson(string keywords, JqGridParam jqgridparam, string ParameterJson, string CompanyID)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                StringBuilder strSql = new StringBuilder();
                List<DbParameter> parameter = new List<DbParameter>();
                strSql.AppendFormat(@" select * from Department where 1=1 and ParentID='{0}' ",CompanyID);
                if (!CommonHelper.IsEmpty(keywords))
                {
                    strSql.AppendFormat(@" and (DepartmentName like '%{0}%' or DepartmentName like '%{0}%') ", keywords);
                }
                if (!ManageProvider.Provider.Current().IsSystem)
                {
                    strSql.AppendFormat(" and companyid='{0}' and IsCompany!=1 ", ManageProvider.Provider.Current().CompanyId);

                }

                DataTable ListData = DepartmentRe.FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);

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

        public ActionResult GridPageUserListJson(string keywords, JqGridParam jqgridparam, string ParameterJson, string CompanyID)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                StringBuilder strSql = new StringBuilder();
                List<DbParameter> parameter = new List<DbParameter>();
                strSql.AppendFormat(@" select a.userid,a.username,a.code,b.departmentname,c.departmentname as companyname, 
 case when a.IsSystem=1 then '管理员' else '一般用户' end as IsSystem,
 case when a.IsAlarm=1 then '是' else '否' end as IsAlarm ,a.AlarmLevel,a.AlarmMail
from BaseUser a 
left join department b on a.DepartmentID=b.DepartmentID 
left join department c on a.companyid=c.DepartmentID
where 1=1 and a.DepartmentID='{0}' ", CompanyID);
                if (!CommonHelper.IsEmpty(keywords))
                {
                    strSql.AppendFormat(@" and (DepartmentName like '%{0}%' or DepartmentName like '%{0}%') ", keywords);
                }

                DataTable ListData = DepartmentRe.FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);

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
        public ActionResult Form()
        {
            return View();
        }

        public ActionResult UserForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetForm(string KeyValue)
        {
            Department entity = DepartmentRe.FindEntity(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            //strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetUserForm(string KeyValue)
        {
            BaseUser entity = UserRe.FindEntity(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            //strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, Department entity, string Detail)
        {
            //string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            //IDatabase database = DataFactory.Database();
            //DbTransaction isOpenTrans = database.BeginTrans();
            try
            {

                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    entity.DepartmentID = KeyValue;
                    
                    if (entity.IsCompany == 1)
                    {
                        entity.CompanyID = entity.DepartmentID;
                    }
                    else
                    {
                        entity.CompanyID = entity.ParentID;
                    }
                    DepartmentRe.Update(entity);
                }
                else
                {
                    entity.DepartmentID = CommonHelper.GetGuid;
                    
                    if(entity.IsCompany==1)
                    {
                        entity.CompanyID = entity.DepartmentID;
                        //entity.ParentID = 0;
                    }
                    else
                    {
                        entity.CompanyID = entity.ParentID;
                    }
                    DepartmentRe.Insert(entity);
                }

               
                return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
            }
            catch (Exception ex)
            {
                //database.Rollback();
                //database.Close();
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        [HttpPost]
        public ActionResult SubmitUserForm(string KeyValue, BaseUser entity, string Detail)
        {
            //string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            //IDatabase database = DataFactory.Database();
            //DbTransaction isOpenTrans = database.BeginTrans();
            try
            {

                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    entity.UserID = KeyValue;
                    UserRe.Update(entity);
                }
                else
                {
                    entity.UserID = CommonHelper.GetGuid;
                    UserRe.Insert(entity);
                }


                return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
            }
            catch (Exception ex)
            {
                //database.Rollback();
                //database.Close();
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        [HttpPost]
        public ActionResult Delete(string KeyValue)
        {
            try
            {
                var Message = "删除失败。";
                int IsOk = 0;

                IsOk = DepartmentRe.Delete(KeyValue);
                DepartmentRe.Delete("DepartmentID", KeyValue);
                if (IsOk > 0)
                {
                    Message = "删除成功。";
                }
                return Content(new JsonMessage { Success = true, Code = IsOk.ToString(), Message = Message }.ToString());
            }
            catch (Exception ex)
            {

                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }
        [HttpPost]
        public ActionResult UserDelete(string KeyValue)
        {
            try
            {
                var Message = "删除失败。";
                int IsOk = 0;

                IsOk = UserRe.Delete(KeyValue);
                DepartmentRe.Delete("UserID", KeyValue);
                if (IsOk > 0)
                {
                    Message = "删除成功。";
                }
                return Content(new JsonMessage { Success = true, Code = IsOk.ToString(), Message = Message }.ToString());
            }
            catch (Exception ex)
            {

                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        public ActionResult CompanyJson()
        {
            string sql = " select * from department where iscompany=1 ";
            if (!ManageProvider.Provider.Current().IsSystem)
            {
                sql = sql + string.Format(" and companyid='{0}'", ManageProvider.Provider.Current().CompanyId);
                //strSql.AppendFormat(" and companyid='{0}' ", ManageProvider.Provider.Current().CompanyId);

            }
            DataTable dt=DepartmentRe.FindTableBySql(sql);
            return Content(dt.ToJson());
        }

        public ActionResult DepartmentJson(string CompanyID)
        {
            string sql = " select * from department where iscompany=0  ";
            string condition = " and ParentID='{0}' ";
            if(!CommonHelper.IsEmpty(CompanyID))
            {
                sql = sql + string.Format(condition, CompanyID);
            }
            if (!ManageProvider.Provider.Current().IsSystem)
            {
                sql = sql + string.Format(" and companyid='{0}'", ManageProvider.Provider.Current().CompanyId);
                //strSql.AppendFormat(" and companyid='{0}' ", ManageProvider.Provider.Current().CompanyId);

            }
            //sql = string.Format(sql, CompanyID);
            DataTable dt = DepartmentRe.FindTableBySql(sql);
            return Content(dt.ToJson());
        }
	}
}