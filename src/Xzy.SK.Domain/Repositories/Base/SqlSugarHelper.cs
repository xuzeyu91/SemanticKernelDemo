using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xzy.SK.Domain.Repositories.Base
{
    public class SqlSugarHelper //不能是泛型类
    {
        /// <summary>
        /// sqlserver连接
        /// </summary>
        public static SqlSugarScope SqlServerDb = new SqlSugarScope(new ConnectionConfig()
        {
            ConnectionString = ConnectionOptions.SqlServer,
            DbType = DbType.SqlServer,
            InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
            IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了
            MoreSettings = new ConnMoreSettings()
            {
                IsWithNoLockQuery = true//看这里
            }
        }, Db =>
        {
            Db.Aop.OnLogExecuting = (sql, pars) =>
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ConvertToString() == "Development")
                {
                    Console.WriteLine(sql + "\r\n" +
                        SqlServerDb.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                    Console.WriteLine();
                }
            };
        });
    }
}
