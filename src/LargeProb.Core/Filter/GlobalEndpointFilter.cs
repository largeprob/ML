using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LargeProb.Core.Filter
{
    /// <summary>
    /// 终结点筛选器(路由位置)
    /// </summary>
    public class GlobalEndpointFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            Console.WriteLine("{MethodName} Before next");
            var result = await next(context);
            Console.WriteLine("{MethodName} After next");
            return result;
        }
    }
}
