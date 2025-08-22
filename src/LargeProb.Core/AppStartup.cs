using LargeProb.Core.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetDemoApp.Core.Filter;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LargeProb.Core
{
    /// <summary>
    /// 应用启动扩展类
    /// </summary>
    public static class AppStartup
    {
        /// <summary>
        /// 控制器
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
        {
            builder.Services
              .AddControllers()
              .AddNewtonsoftJson(options =>
              {
                  //格式化日期响应格式
                  options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                  //格式化属性命名方式-小驼峰
                  options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
              });
            return builder;
        }

        /// <summary>
        /// 跨域策略
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder AddCors(this WebApplicationBuilder builder)
        {   
            Dictionary<string, string[]> cors = new Dictionary<string, string[]>();
            builder.Configuration.GetSection("Cors").Bind(cors);
            builder.Services.AddCors(options =>
            {
                foreach (var item in cors)
                {
                    options.AddPolicy(item.Key, builder =>
                    {
                        builder.WithOrigins(item.Value.ToArray())
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
                }
            });
            return builder;
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder AppSettingsConfig(this WebApplicationBuilder builder)
        {
            string rootPath = Directory.GetCurrentDirectory() + "//ConfigJson";
            FileInfo[] files = new DirectoryInfo(rootPath).GetFiles().Where(x => !(x.FullName.IndexOf($"{builder.Environment.EnvironmentName}") == -1)).ToArray();
            foreach (var file in files)
            {
                builder.Configuration.AddJsonFile($"{file.FullName}", optional: true, reloadOnChange: true);
            }
            return builder;
        }

        /// <summary>
        /// 添加JWT身份验证
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="schemesName">策略名称</param>
        /// <returns></returns>
        public static WebApplicationBuilder AddAuthenJwt(this WebApplicationBuilder builder,string schemesName)
        {
            var config = builder.Configuration.GetSection("Authentication:Schemes:" + schemesName);

            //RSA加密
            var rsaPublicKey = RSA.Create();
            rsaPublicKey.ImportRSAPublicKey(Convert.FromBase64String(config["RSA:PublicKey"]), out _);
            builder.Services.AddAuthentication().AddJwtBearer(options =>
            {
                //这里只配置密钥，其他值会被系统在配置文件中自动初始化<see cref="JwtBearerConfigureOptions"/>
                options.TokenValidationParameters.IssuerSigningKey = new RsaSecurityKey(rsaPublicKey);
                options.Events = new JwtBearerEvents
                {
                    //授权失败全部返回401
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync("身份未授权");
                        //return Task.CompletedTask;
                    }
                };
            });
            return builder;
        }

        /// <summary>
        /// 添加API鉴权策略
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="policyName">自定义的策略名称</param>
        /// <returns></returns>
        /// <remarks>
        /// <see cref="APIAccessRequirement"/>对于API鉴权功能没有意义，因为我们的权限持久化在数据库中。这里只是为了为了触发<see cref="APIAccessHandler"/>
        /// </remarks>
        public static WebApplicationBuilder AddAuthorAPI(this WebApplicationBuilder builder, string policyName)
        {
            //builder.Services.AddAuthorization(options =>
            //{
            //    options.AddPolicy(policyName, policy => policy.Requirements.Add(new APIAccessRequirement()));
            //});
            //builder.Services.AddSingleton<IAuthorizationHandler, APIAccessHandler>();
            //builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, GlobalAuthorResultHandler>();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "your-issuer",
                    ValidAudience = "your-audience",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key"))
                };
            });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(policyName, policy => policy.Requirements.Add(new APIAccessRequirement()));

                options.AddPolicy("DeleteExpressCompanyProductWeight", policy =>
                {
                    policy.RequireClaim("Permission", "BasicData.ExpressCompanyProductWeight.Delete");
                });
            });
            builder.Services.AddSingleton<IAuthorizationHandler, APIAccessHandler>();
            builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, GlobalAuthorResultHandler>();

            return builder;

        }


        /// <summary>
        /// 添加全局Http请求日志
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder AddW3CLogging(this WebApplicationBuilder builder)
        {
            builder.Services.AddW3CLogging(logging =>
            {
                // Log all W3C fields
                logging.LoggingFields = W3CLoggingFields.All;

                logging.AdditionalRequestHeaders.Add("x-forwarded-for");
                logging.AdditionalRequestHeaders.Add("x-client-ssl-protocol");
                logging.FileSizeLimit = 5 * 1024 * 1024;
                logging.RetainedFileCountLimit = 2;
                logging.FileName = "MyLogFile";
                logging.LogDirectory = Directory.GetCurrentDirectory() + $"//logs//W3CLogs//"; ;
                logging.FlushInterval = TimeSpan.FromSeconds(2);
            });
            return builder;
        }


        /// <summary>
        /// 添加全局Http请求日志
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder AddHttpLogging(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpLogging((logging) =>
            {
                //仅记录请求|持续时间|响应头
                logging.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.Duration | HttpLoggingFields.ResponsePropertiesAndHeaders;
                //需要记录的最大请求长度单位：默认 32 KB. 
                logging.RequestBodyLogLimit = 32 * 1024;
                //将 CombineLogs 设置为 true 会配置中间件，使其在最后将请求和响应所有已启用的日志合并到一个日志中。 这包括请求、请求正文、响应、响应正文和持续时间。
                logging.CombineLogs = true;
            });

            builder.Services.AddHttpLoggingInterceptor<SampleHttpLoggingInterceptor>();
            return builder;
        }
 
        /// <summary>
        /// 添加全局过滤器
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder AddGlobalFilter(this WebApplicationBuilder builder)
        {
            Action<MvcOptions> setupAction = (options) =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
                options.Filters.Add<GlobalActionFilter>();
            };
            builder.Services.Configure(setupAction);
            return builder;
        }

        /// <summary>
        /// 关闭全局模型自动验证
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder CloseModelInvalid(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<ApiBehaviorOptions>((o) =>
            {
                o.SuppressModelStateInvalidFilter = true;
            });
            return builder;
        }

        /// <summary>
        /// 注册Swagger
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebApplicationBuilder AddSwaggerGen(this WebApplicationBuilder builder, string apiXML,bool hasAuth = false)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("后台接口", new OpenApiInfo
                {
                    Version = "1.0",
                    Title = "后台接口",
                    Description = $"API描述",
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, apiXML), true);

                if (hasAuth)
                {
                    c.AddSecurityDefinition("JwtBearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT授权",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Scheme = "bearer",
                        Type = SecuritySchemeType.Http,
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme{
                        Reference=new OpenApiReference{ Type=ReferenceType.SecurityScheme,Id="JwtBearer"}
                    }, new List<string>() }});

                }
            });

            return builder;
        }

        /// <summary>
        /// 注册基础库数据上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static WebApplicationBuilder AddDataAccess<T>(this WebApplicationBuilder builder) where T:DbContext
        {
            var str = builder.Configuration.GetConnectionString("blog") ?? throw new Exception("Connection string 'blog' not found.");
            builder.Services.AddDbContextPool<T>((o) =>
            {
                o.UseSqlServer(str);
                //var loggerFactory = new LoggerFactory();
                //o.UseLoggerFactory(loggerFactory).EnableSensitiveDataLogging();
            }, 60);

            //        builder.EnrichSqlServerDbContext<ExampleDbContext>(
            //configureSettings: settings =>
            //{
            //    settings.DisableRetry = false;
            //    settings.CommandTimeout = 30; // seconds
            //});

            return builder;

        }

     
    }

    public class SampleHttpLoggingInterceptor : IHttpLoggingInterceptor
    {
        public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
        {
            return default;
        }

        public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
        {
            return default;
        }
    }
}
