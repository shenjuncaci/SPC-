using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Repository;
using Utilities;

namespace SPCSystem.Controllers
{
    [LoginAuthorize]
    public class AlarmController : Controller
    {
        Repository<Alarm> productre = new Repository<Alarm>();
        // GET: Alarm
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
                strSql.Append(@" select * from alarm where 1=1   ");
                if (!CommonHelper.IsEmpty(keywords))
                {
                    strSql.AppendFormat(@" and (alarmname like '%{0}%') ", keywords);
                }
                if (ManageProvider.Provider.Current().IsSystem == false)
                {
                    strSql.AppendFormat(@" and CompanyID='{0}' ", ManageProvider.Provider.Current().CompanyId);
                }

                DataTable ListData = productre.FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);

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

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetForm(string KeyValue)
        {
            Alarm entity = productre.FindEntity(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            //strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, Alarm entity, string Detail)
        {
            try
            {

                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    entity.AlarmID = KeyValue;
                    productre.Update(entity);
                }
                else
                {
                    entity.AlarmID = CommonHelper.GetGuid;
                    productre.Insert(entity);
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

                IsOk = productre.Delete(KeyValue);
               // productre.Delete("DepartmentID", KeyValue);
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
    }
}