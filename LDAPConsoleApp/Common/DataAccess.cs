using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDAPConsoleApp.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class DataAccess
    {
        /// <summary>
        /// default 连接
        /// </summary>
        private static string SqlServerConnStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();

        /// <summary>
        /// 默认SqlServer数据库连接
        /// </summary>
        public static SqlSugarClient Default
        {
            get
            {
                return new SqlSugarClient(new ConnectionConfig()
                {
                    //必填, 数据库连接字符串
                    ConnectionString = SqlServerConnStr,
                    //必填, 数据库类型
                    DbType = DbType.SqlServer,
                    //默认false, 时候知道关闭数据库连接, 设置为true无需使用using或者Close操作
                    IsAutoCloseConnection = true,
                    //默认SystemTable, 字段信息读取, 如：该属性是不是主键，是不是标识列等等信息
                    InitKeyType = InitKeyType.SystemTable,
                    //缓存
                    ConfigureExternalServices = new ConfigureExternalServices()
                    {
                        DataInfoCacheService = new HttpRuntimeCache() 
                    }
                });
            }
        }
    }
}
