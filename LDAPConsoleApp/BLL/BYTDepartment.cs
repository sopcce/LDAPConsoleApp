using LDAPConsoleApp.Common;
using LDAPConsoleApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDAPConsoleApp.BLL
{
    public class BYTDepartment
    {
        /// <summary>
        /// 获取部门完整路径列表
        /// </summary>
        /// <returns></returns>
        public List<YTFullDept> GetFullDeptList(string strWhere)
        {
            var db = DataAccess.Default;
            var query = db.SqlQueryable<YTFullDept>("select * from v_FullValidDept")
                .WhereIF(strWhere.IsNotNullOrEmpty(), "({0})".FormatWith(strWhere))
                .ToList();

            return query;
        }
    }
}
