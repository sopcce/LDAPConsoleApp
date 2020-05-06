using LDAPConsoleApp.BLL;
using LDAPConsoleApp.Common;
using LDAPConsoleApp.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDAPConsoleApp
{
    class Program
    {
        
        static void Main(string[] args)
        {
            MainHelper aminHelper = new MainHelper();

            //DirectoryEntry de = aminHelper.GetDirectoryEntry();
            DirectoryEntry de = aminHelper.GetDirectoryEntryOfUser("");
            aminHelper.SetPassword(de, "123Qweasd");

            //同步部门
            //BYTDepartment bd = new BYTDepartment();
            //List<YTFullDept> lstFullDept = bd.GetFullDeptList("");
            //aminHelper.SyncOU(lstFullDept, lstFullDept);

            ////同步人员信息
            //BYTStaff bs = new BYTStaff();
            //List<YTStaff> lstStaff = bs.GetStaffList(" StaffDepartment = 1056 and staffstate = 1 ");
            //aminHelper.SyncUser(lstFullDept, lstStaff);
        }
    }
}
