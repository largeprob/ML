using LargeProb.Core.Controller;
using LargeProb.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Net;
 

namespace NetDemoApp.Core.Filter
{
    /// <summary>
    /// 全局控制器异常处理过滤器
    /// </summary>
    ///<remarks>
    ///关于过滤器的交互方式 https://learn.microsoft.com/zh-cn/aspnet/core/mvc/controllers/filters?view=aspnetcore-7.0
    /// </remarks>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostEnvironment HostEnvironment;
        private ILogger Logger;

        public GlobalExceptionFilter(IHostEnvironment hostEnvironment, ILogger<GlobalExceptionFilter> logger)
        {
            HostEnvironment = hostEnvironment;
            Logger = logger;
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            //如果是开发环境，直接将错误返回。是自定义异常类型，将错误返回客户端提示
            if (HostEnvironment.IsDevelopment() || context.Exception is SolutionException)
            {
                context.HttpContext.Response.StatusCode = 200;
                context.Result = new JsonResult(new SolutionResult(HttpStatusCode.BadRequest, context.Exception.Message));
                context.ExceptionHandled = true;
                return;
            }

           
            //如果走到下面都是系统级别异常

            //如果是开发环境，直接将错误返回。服务器级异常
            //if (HostEnvironment.IsDevelopment()) return;

            //如果是其他环境，我们将错误打印在控制台/存在本地文件/存放在数据库中
            if (HostEnvironment.IsProduction())
            {
                //这个库的名称叫做 Spectre.Console 能够帮助打印出各种图形等
                //.NET Core中打印日志/记录日志的方式有N种，不必纠结于使用哪种方式。只需要适合当前情况的
                var rows = new List<Text>(){
                new Text($"\n──异常信息────────────────────────────────────────────────────", new Style(Color.Red1)),
                new Text($"发生时间：{string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", DateTime.Now)}", new Style(Color.Red1)),
                new Text($"发生用户：管理员", new Style(Color.Red1)),
                new Text($"异常内容：{context.Exception.Message}", new Style(Color.Red1)),
                new Text($"调用堆栈：{context.Exception.StackTrace} ",new Style(Color.Red1)),
                new Text($"───────────────────────────────────────────────────────────", new Style(Color.Red1)),
                };
                AnsiConsole.Write(new Rows(rows));

                //正确处理异常返回
                context.HttpContext.Response.StatusCode = 200;
                context.Result = new JsonResult(new SolutionResult(HttpStatusCode.BadRequest, "系统异常"));
                context.ExceptionHandled = true;
                return;
            }
        }
    }
}
