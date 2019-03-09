//=====================================================================================
// All Rights Reserved , Copyright @ Excalibur 2014
// Software Developers @ Excalibur 2014
//=====================================================================================

using DataAccess;
using Repository;
using Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace SPCSystem
{
    /// <summary>
    /// 用户管理
    /// <author>
    ///		<name>shenjun</name>
    ///		<date>2018.12.26 15:45</date>
    /// </author>
    /// </summary>
    public class Base_UserBll
    {
        Repository<BaseUser> re = new Repository<BaseUser>();
        /// <summary>
        /// 判断是否连接服务器
        /// </summary>
        /// <returns></returns>
        public bool IsLinkServer()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT  GETDATE()");
            DataTable dt = re.FindTableBySql(strSql.ToString());
            if (dt != null && dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 登陆验证信息
        /// </summary>
        /// <param name="Account">账户</param>
        /// <param name="Password">密码</param>
        /// <param name="result">返回结果</param>
        /// <returns></returns>
        public BaseUser UserLogin(string Account, string Password, out string result)
        {
            if (!this.IsLinkServer())
            {
                throw new Exception("服务器连接不上，" + DbResultMsg.ReturnMsg);
            }
            BaseUser entity = re.FindEntity("Code", Account);
            if (entity != null && entity.UserID != null)
            {
                if (Password == entity.Password)
                {
                    result = "succeed";
                }
                else
                {
                    result = "error";
                }
                return entity;
            }
            result = "-1";
            return null;
        }

        public int ChangePassword(string NewPassword)
        {
            StringBuilder strsql = new StringBuilder();
            strsql.AppendFormat(@" update BaseUser set Password='{0}' where UserID='{1}'  ",NewPassword,ManageProvider.Provider.Current().UserId);
            return re.ExecuteBySql(strsql);
        }
        /// <summary>
        /// 获取用户角色列表
        /// </summary>
        /// <param name="CompanyId">公司ID</param>
        /// <param name="UserId">用户Id</param>
        /// <returns></returns>
        public DataTable UserRoleList(string CompanyId, string UserId,string RoleName)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"SELECT  r.RoleId ,				--角色ID
                                    r.Code ,				--编码
                                    r.FullName ,			--名称
                                    r.SortCode ,			--排序码
                                    ou.ObjectId				--是否存在
                            FROM    Base_Roles r
                                    LEFT JOIN Base_ObjectUserRelation ou ON ou.ObjectId = r.RoleId
                                                                            AND ou.UserId = @UserId
                                                                            AND ou.Category = 2
                            WHERE 1 = 1");
            //if (!ManageProvider.Provider.Current().IsSystem)
            //{
            //    strSql.Append(" AND ( RoleId IN ( SELECT ResourceId FROM Base_DataScopePermission WHERE");
            //    strSql.Append(" ObjectId IN ('" + ManageProvider.Provider.Current().ObjectId.Replace(",", "','") + "') ");
            //    strSql.Append(" ) )");
            //}
            //strSql.Append(" AND r.CompanyId = @CompanyId");
            if(RoleName== "车间主任")
            {
                strSql.Append(" and r.RoleId='f6afd4e4-6fb2-446f-88dd-815ddb91b09d' ");
            }
            parameter.Add(DbFactory.CreateDbParameter("@UserId", UserId));
            //parameter.Add(DbFactory.CreateDbParameter("@CompanyId", CompanyId));
            return re.FindTableBySql(strSql.ToString(), parameter.ToArray());
        }
        /// <summary>
        /// 选择用户列表
        /// </summary>
        /// <param name="keyword">模块查询</param>
        /// <returns></returns>
        public DataTable OptionUserList(string keyword)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@"SELECT TOP 50 * FROM ( SELECT    
                                        u.UserId ,
                                        u.Account ,
                                        u.code,
                                        u.RealName ,
                                        u.DepartmentId ,
                                        d.FullName AS DepartmentName,
                                        u.Gender
                                FROM    Base_User u
                                LEFT JOIN Base_Department d ON d.DepartmentId = u.DepartmentId
                                WHERE   u.RealName LIKE @keyword
                                        OR u.Account LIKE @keyword
                                        OR u.Code LIKE @keyword
                                        OR u.Spell LIKE @keyword
                                        OR u.UserId IN (
                                        SELECT  u.UserId
                                        FROM    Base_User u
                                                INNER JOIN Base_ObjectUserRelation oc ON u.UserId = oc.UserId
                                                INNER JOIN dbo.Base_Company c ON c.CompanyId = oc.ObjectId
                                        WHERE   c.FullName LIKE @keyword
                                        UNION
                                        SELECT  u.UserId
                                        FROM    Base_User u
                                                INNER JOIN Base_ObjectUserRelation od ON u.UserId = od.UserId
                                                INNER JOIN Base_Department d ON d.DepartmentId = od.ObjectId
                                        WHERE   d.FullName LIKE @keyword
                                        UNION
                                        SELECT  u.UserId
                                        FROM    Base_User u
                                                INNER JOIN Base_ObjectUserRelation oro ON u.UserId = oro.UserId
                                                INNER JOIN Base_Roles r ON r.RoleId = oro.ObjectId
                                        WHERE   r.FullName LIKE @keyword
                                        UNION
                                        SELECT  u.UserId
                                        FROM    Base_User u
                                                INNER JOIN Base_ObjectUserRelation op ON u.UserId = op.UserId
                                                INNER JOIN Base_Post p ON p.PostId = op.ObjectId
                                        WHERE   p.FullName LIKE @keyword
                                        UNION
                                        SELECT  u.UserId
                                        FROM    Base_User u
                                                INNER JOIN Base_ObjectUserRelation og ON u.UserId = og.UserId
                                                INNER JOIN Base_GroupUser g ON g.GroupUserId = og.ObjectId
                                        WHERE   g.FullName LIKE @keyword )
                            ) a WHERE 1 = 1");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            else
            {
                strSql.Append(@"SELECT TOP 50
                                        u.UserId ,
                                        u.Account ,
                                        u.code ,
                                        u.RealName ,
                                        u.DepartmentId ,
                                        d.FullName AS DepartmentName ,
                                        u.Gender
                                FROM    Base_User u
                                        LEFT JOIN Base_Department d ON d.DepartmentId = u.DepartmentId
                                WHERE   1 = 1");
            }
            if (!ManageProvider.Provider.Current().IsSystem)
            {
                strSql.Append(" AND ( UserId IN ( SELECT ResourceId FROM Base_DataScopePermission WHERE");
                strSql.Append(" ObjectId IN ('" + ManageProvider.Provider.Current().ObjectId.Replace(",", "','") + "') ");
                strSql.Append(" ) )");
            }
            return re.FindTableBySql(strSql.ToString(), parameter.ToArray());
        }

        
    }
}