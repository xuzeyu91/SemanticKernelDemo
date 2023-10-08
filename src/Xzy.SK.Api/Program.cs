using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using Xzy.SK.Domain;

namespace Xzy.SK
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var appsettings = GetAppSettings();
            var configuration = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory)
                                           //.AddJsonFile("appsettings.json")
                                           .AddJsonStream(new MemoryStream(appsettings))
                                           .Build();
            var url = configuration["urls"];
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonStream(new MemoryStream(appsettings));
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(url);
                    webBuilder.UseStartup<Startup>();
                });
        }

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        /// <returns></returns>
        private static byte[] GetAppSettings()
        {
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ConvertToString();
            //读取configmap。写入到配置文件
            var builderEnv = new ConfigurationBuilder().AddEnvironmentVariables();
            IConfiguration Configuration = builderEnv.Build();
            //获取configmap
            string appsettings = Configuration.GetValue<string>("appsettings.json");
            //获取
            if (appsettings.ConvertToString() == "")
            {
                if (envName == "Production")
                {
                    appsettings = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "appsettings.json"));
                }
                else
                {
                    appsettings = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"appsettings.{envName}.json"));
                }
            }
            appsettings = InitSecrect(appsettings);
            return System.Text.Encoding.UTF8.GetBytes(appsettings);
        }

        //通过环境变量Secret替换appsetting
        private static string InitSecrect(string appsettings)
        {
            var envList = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry env in envList)
            {
                if (env.Key.ConvertToString().Contains("._"))
                {
                    appsettings = appsettings.Replace(env.Key.ConvertToString(), env.Value.ConvertToString());
                    Console.WriteLine($"替换Screct:{env.Key.ConvertToString()}");
                }
            }
            return appsettings;
        }
    }
}
