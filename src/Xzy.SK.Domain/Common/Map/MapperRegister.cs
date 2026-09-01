using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Xzy.SK.Domain.Common.Map
{
    public static class MapperRegister
    {
        /// <summary>
        /// 注册 AutoMapper
        /// AutoMapper 15 起的变化：
        /// 1. 移除静态 Mapper API，统一使用实例 IMapper；
        /// 2. MapperConfiguration 构造函数需要 ILoggerFactory；
        /// 3. 商业授权，需要 LicenseKey（配置 AutoMapper:LicenseKey 或环境变量 AUTOMAPPER_LICENSE_KEY），
        ///    未配置时 AutoMapper 仅记录 WARNING 日志，不做功能降级。
        /// </summary>
        public static IServiceCollection AddMapper(
            this IServiceCollection services,
            string? licenseKey = null,
            ILoggerFactory? loggerFactory = null)
        {
            licenseKey ??= Environment.GetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY");

            var config = new MapperConfiguration(cfg =>
            {
                if (!string.IsNullOrWhiteSpace(licenseKey))
                {
                    cfg.LicenseKey = licenseKey;
                }

                cfg.AddProfile<AutoMapProfile>();
            }, loggerFactory ?? NullLoggerFactory.Instance);

            IMapper mapper = config.CreateMapper();

            //启动实体映射
            MapperExtend.UseMapper(mapper);

            services.AddSingleton(mapper);

            return services;
        }
    }
}
