using Xzy.SK.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xzy.SK.Domain.Common.Map;
using Xzy.SK.Domain.Common.Options;
using FluentValidation.AspNetCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Xzy.SK.Domain.Common.Utils;
using Microsoft.SemanticKernel;

namespace Xzy.SK
{
    public class Startup 
    {

        public IHostEnvironment Env { get; set; }
        private readonly string Any = "Any";
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(config =>
            {
                //此设定解决JsonResult中文被编码的问题
                config.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

                config.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                config.JsonSerializerOptions.Converters.Add(new DateTimeNullableConvert());
            });

            //注入配置文件
            InitConfig();

            //反射根据特性依赖注入
            services.AddServicesFromAssemblies("Xzy.SK.Domain");

            //swagger
            InitSwagger(services);
            //Mapper
            services.AddMapper();
        
            //允许跨域
            services.AddCors(options => options.AddPolicy(Any,
               builder =>
               {
                   builder.AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetIsOriginAllowed(_ => true)
                       .AllowCredentials();
               }));
            //用户service中获取httpcontext
            services.AddHttpContextAccessor();
            //初始化模型验证
            InitValidation(services);

            InitSK(services);
        }


        private static void InitSK(IServiceCollection services)
        {
            services.AddTransient<IKernel>((serviceProvider) =>
            {
                return Kernel.Builder
                .WithAzureChatCompletionService(
                     OpenAIOptions.Model,
                     OpenAIOptions.Endpoint,
                     OpenAIOptions.Key)
                .Build();
            });
        }


        /// <summary>
        /// 初始化模型验证
        /// </summary>
        /// <param name="services"></param>
        private static void InitValidation(IServiceCollection services)
        {
            //模型验证
            services.AddControllersWithViews()
            .AddFluentValidation(config =>//添加FluentValidation验证
            {

                //程序集方式添加验证
                // config.RegisterValidatorsFromAssemblyContaining(typeof(TMTotalCostGenerateDetailValidation));
                //注入程序集
                config.RegisterValidatorsFromAssembly(Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name));
                config.RegisterValidatorsFromAssembly(Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name.Replace("Api", "Domain")));
                //是否与MvcValidation共存，设置为false后将不再执行特性方式的验证
                //config.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
            });
            //重写模型验证错误
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (context) =>
                {
                    var errors = context.ModelState
                        .Values
                        .SelectMany(x => x.Errors
                                    .Select(p => p.ErrorMessage))
                        .ToList();

                    var result = new
                    {
                        code = "400",
                        message = "Validation errors",
                        data = errors
                    };

                    return new BadRequestObjectResult(result);
                };
            });
        }
   

        /// <summary>
        /// 初始化swagger
        /// </summary>
        /// <param name="services"></param>
        private static void InitSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SK API", Version = "v1" });
                //添加Api层注释（true表示显示控制器注释）
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true);
                //添加Domain层注释（true表示显示控制器注释）
                var xmlFile1 = $"{Assembly.GetExecutingAssembly().GetName().Name.Replace("Api", "Domain")}.xml";
                var xmlPath1 = Path.Combine(AppContext.BaseDirectory, xmlFile1);
                c.IncludeXmlComments(xmlPath1, true);
                c.DocInclusionPredicate((docName, apiDes) =>
                {
                    if (!apiDes.TryGetMethodInfo(out MethodInfo method))
                        return false;
                    var version = method.DeclaringType.GetCustomAttributes(true).OfType<ApiExplorerSettingsAttribute>().Select(m => m.GroupName);
                    if (docName == "v1" && !version.Any())
                        return true;
                    var actionVersion = method.GetCustomAttributes(true).OfType<ApiExplorerSettingsAttribute>().Select(m => m.GroupName);
                    if (actionVersion.Any())
                        return actionVersion.Any(v => v == docName);
                    return version.Any(v => v == docName);
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "Directly enter bearer {token} in the box below (note that there is a space between bearer and token)",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference()
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        }, Array.Empty<string>()
                    }
                });
            });
        }

        /// <summary>
        /// 注入配置文件
        /// </summary>
        private void InitConfig()
        {
            Configuration.GetSection("OpenAI").Get<OpenAIOptions>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
          
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            //跨域
            app.UseCors(Any);

            app.UseAuthorization();

            if (!env.IsProduction())
            {
                //启用Swagger
                app.UseSwagger();
                //配置Swagger UI
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SK API"); //注意中间段v1要和上面SwaggerDoc定义的名字保持一致
                });
            }
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
