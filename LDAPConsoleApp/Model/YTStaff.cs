using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDAPConsoleApp.Model
{
    public class YTStaff
    {
        public int staffID { get; set; }

        public string staffNum { get; set; }

        public string StaffName { get; set; }

        public string StaffSex { get; set; }

        public string StaffDepartMent { get; set; }
        /// <summary>
        /// 工作座机
        /// </summary>
        public string StaffTel { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string StaffPhone { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string StaffEmail { get; set; }
        public string StaffState { get; set; }
        public DateTime? StaffCreateDate { get; set; }
        public int PartID { get; set; }
        public string PartName { get; set; }
    }
}
