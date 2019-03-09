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
    public class ProductController : Controller
    {
        Repository<Product> productre = new Repository<Product>();
        Repository<CalMethod> calre = new Repository<CalMethod>();
        Repository<CalCriterion> crire = new Repository<CalCriterion>();
        //
        // GET: /Product/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords,JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                StringBuilder strSql = new StringBuilder();
                List<DbParameter> parameter = new List<DbParameter>();
                strSql.Append(@" select * from product where 1=1   ");
                if(!CommonHelper.IsEmpty(keywords))
                {
                    strSql.AppendFormat(@" and (partname like '%{0}%' or productno like '%{0}%') ",keywords);
                }
                if(ManageProvider.Provider.Current().IsSystem==false)
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
            Product entity = productre.FindEntity(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            //strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        public ActionResult GridPageListJson2(string MID)
        {
            string sql = " select * from CalMethod where ProductID='" + MID + "' ";
            DataTable dt = productre.FindTableBySql(sql);
            return Content(dt.ToJson());
        }

        public ActionResult GridPageListJson3(string MID)
        {
            string sql = " select * from CalCriterion where ProductID='" + MID + "' ";
            DataTable dt = productre.FindTableBySql(sql);
            return Content(dt.ToJson());
        }

        public ActionResult GridPageListJsonBasic()
        {
            string sql = " select * from Criterion order by CriterionNo  ";
            DataTable dt = productre.FindTableBySql(sql);
            return Success(dt);
        }

        public ActionResult Success(object data)
        {
            return Content(new ResParameter { code = ResponseCode.success, info = "响应成功", data = data }.ToJson());
        }

        public enum ResponseCode
        {
            /// <summary>
            /// 成功
            /// </summary>
            success = 200,
            /// <summary>
            /// 失败
            /// </summary>
            fail = 400,
            /// <summary>
            /// 异常
            /// </summary>
            exception = 500
        }
        public class ResParameter
        {
            /// <summary>
            /// 接口响应码
            /// </summary>
            public ResponseCode code { get; set; }
            /// <summary>
            /// 接口响应消息
            /// </summary>
            public string info { get; set; }
            /// <summary>
            /// 接口响应数据
            /// </summary>
            public object data { get; set; }
        }


        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, Product entity, string Detail,string Detail2)
        {
            try
            {
                //第一步就先尝试转换明细表的数据
                List<CalMethod> list = Detail.JonsToList<CalMethod>();
                List<CalCriterion> list2 = Detail2.JonsToList<CalCriterion>();
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    entity.ProductID = KeyValue;
                    //entity.CompanyID = ManageProvider.Provider.Current().CompanyId;
                    productre.Update(entity);
                }
                else
                {
                    entity.ProductID = CommonHelper.GetGuid;
                    entity.CompanyID = ManageProvider.Provider.Current().CompanyId;
                    productre.Insert(entity);
                }

                
                calre.Delete("ProductID", entity.ProductID);
                crire.Delete("ProductID", entity.ProductID);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].CalID = CommonHelper.GetGuid;
                    list[i].ProductID = entity.ProductID;
                    calre.Insert(list[i]);
                }
                for (int i = 0; i < list2.Count; i++)
                {
                    list2[i].CalCriterionID = CommonHelper.GetGuid;
                    list2[i].ProductID = entity.ProductID;
                    crire.Insert(list2[i]);
                }
                //isOpenTrans.Commit();
                //isOpenTrans.Dispose();
                return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
            }
            catch (Exception ex)
            {
                //isOpenTrans.Rollback();
                //isOpenTrans.Dispose();
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
                //删除明细表的数据
                calre.Delete("ProductID", KeyValue);
                crire.Delete("ProductID", KeyValue);
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