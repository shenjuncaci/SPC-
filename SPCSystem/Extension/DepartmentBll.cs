using Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace SPCSystem
{
    public class DepartmentBll
    {
        public DataTable GetTree(Repository<Department> DepartmentRe)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@" select * from Department where 1=1 ");
            if (!ManageProvider.Provider.Current().IsSystem)
            {
                strSql.AppendFormat(" and companyid='{0}' ",ManageProvider.Provider.Current().CompanyId);
               
            }
            strSql.Append(" ORDER BY SortNo ASC");
            return DepartmentRe.FindTableBySql(strSql.ToString());
        }
    }
}