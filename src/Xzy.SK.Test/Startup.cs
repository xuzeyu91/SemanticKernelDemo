using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using Xunit;

namespace Xzy.SK.Test
{
    public class Startup
    {
        // 自定义 host 构建
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureAppConfiguration(builder =>
                {
                    // 注册配置
                    builder.AddJsonFile("appsettings.Development.json");
                })
                .ConfigureServices((context, services) =>
                {
                    //// 注册自定义服务
                    //services.AddServicesFromAssemblies("Xzy.Project.Domain");

                }).ConfigureWebHostDefaults(webBuilder =>
                {
                    //启动Api中的依赖注入、初始化等操作
                    webBuilder.UseStartup<Xzy.SK.Startup>();
                });

        }

        // 可以添加要用到的方法参数，会自动从注册的服务中获取服务实例，类似于 asp.net core 里 Configure 方法
        public void Configure(IServiceProvider applicationServices)
        {
            // 有一些测试数据要初始化可以放在这里

        }
    }
}
