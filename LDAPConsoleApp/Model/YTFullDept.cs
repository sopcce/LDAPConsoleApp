using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDAPConsoleApp.Model
{
    public class YTFullDept
    {
        /// <summary>
        /// 部门id
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 父部门id
        /// </summary>
        public string parentId { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; }
        /// <summary>
        /// 部门全路径
        /// </summary>
        public string FullName { get; set; }
    }
}
