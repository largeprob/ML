using LargeProb.Core.Controller;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spectre.Console;
using System.Diagnostics;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetDemoApp.Core.Filter
{
    /// <summary>
    /// 全局控制器方法处理拦截器
    /// </summary>
    ///<remarks>
    ///关于过滤器的交互方式 https://learn.microsoft.com/zh-cn/aspnet/core/mvc/controllers/filters?view=aspnetcore-8.0
    /// </remarks>
    public class GlobalActionFilter : IAsyncActionFilter
    {
        /// <summary>
        /// 在绑定模型后，执行方法之前
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();


            var modelValid = ModelValid(context, next);
            if (!modelValid.success)
            {
                context.HttpContext.Response.StatusCode = 200;
                context.Result = new JsonResult(new SolutionResult(HttpStatusCode.BadRequest, modelValid.error));
                return;
            }
            await next();

            sw.Stop();

            var request = context.HttpContext.Request;
            var rows = new List<Text>(){
                new Text($"\n──接口耗时────────────────────────────────────────────────────", new Style(Color.Green)),
                //new Text($"发生时间：{string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", DateTime.Now)}", new Style(Color.Green)),
                new Text($"调用者：管理员", new Style(Color.Green)),
                new Text($"接口：{request.Method} {request.Scheme}://{ request.Host+request.Path} { request.Protocol }", new Style(Color.Green)),
                //new Text($"调用堆栈：{context.Exception.StackTrace} ",new Style(Color.Green)),
                new Text($"总耗时：{sw.Elapsed}s; ",new Style(Color.Green)),
                new Text($"───────────────────────────────────────────────────────────", new Style(Color.Green)),
            };
            AnsiConsole.Write(new Rows(rows));
        }

        /// <summary>
        /// 模型验证
        /// </summary>
        private (bool success, string? error) ModelValid(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 注释：用来验证模型是否通过
            if (context.ModelState.IsValid) return (true, default);


            /*
             * 摘要：获取没有通过验证的错误提示
             * 
             * 注释： SelectMany 是LINQ 的一个查询方法用来将返回序列变成一个单独的序列
             */
            var errorMessage = context.ModelState.Values.Select(x => x.Errors).SelectMany(x => x.Select(x => x.ErrorMessage)).FirstOrDefault();
            return (false, errorMessage);
        }
    }
 
}
