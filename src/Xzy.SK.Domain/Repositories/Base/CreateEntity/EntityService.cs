using Xzy.SK.Domain.Repositories;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xzy.SK.Domain.Repositories
{
    [ServiceDescription(typeof(IEntityService), ServiceLifetime.Scoped)]
    public class EntityService:IEntityService
    {
        public SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = ConnectionOptions.SqlServer,
                DbType = DbType.SqlServer,
                InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了
            }); 
        /// <summary>
        /// 生成实体类
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool CreateEntity(string entityName, string filePath)
        {
            try
            {
                db.DbFirst.IsCreateAttribute().Where(entityName).SettingClassTemplate(old =>
                {
                    return old.Replace("{Namespace}", "Xzy.SK.Domain.Repositories");//修改Namespace命名空间
                }).CreateClassFile(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
