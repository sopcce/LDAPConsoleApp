using LDAPConsoleApp.Common;
using LDAPConsoleApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDAPConsoleApp.BLL
{
    public class BYTStaff
    {
        public List<YTStaff> GetStaffList(string strWhere)
        {
            var db = DataAccess.Default;
            var query = db.SqlQueryable<YTStaff>("select staffID,staffNum,StaffName,StaffSex,StaffDepartMent,StaffTel,StaffPhone,StaffEmail,StaffState,StaffCreateDate,PartID,PartName from YTStaff")
                .Where(e=>e.StaffState == "1")
                .WhereIF(strWhere.IsNotNullOrEmpty(), "({0})".FormatWith(strWhere))
                .ToList();

            return query;
        }
    }
}
