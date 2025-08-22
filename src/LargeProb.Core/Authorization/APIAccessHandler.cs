using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LargeProb.Core.Authorization
{

    /// <summary>
    /// API访问权限验证
    /// </summary>
    public class APIAccessHandler : AuthorizationHandler<APIAccessRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, APIAccessRequirement requirement)
        {
            //没有身份信息，说明未通过身份验证或不需要验证
            if (context.User.Identity.IsAuthenticated == false)
            {
                return Task.CompletedTask;
            }


            //验证是否有此接口是否存在于用户权限之中
            //var httpContext = context.Resource as DefaultHttpContext;
            //var end = httpContext.GetEndpoint();
            //var funcs = end.Metadata.GetMetadata<AuthorizeFunctionAttribute>();
            //var clis = context.User.Claims;
            //if (!clis.Any(x => x.Type == "fun" && funcs.Funcs.Contains(x.Value)))
            //{
            //    context.Fail(new AuthorizationFailureReason(this, "无法访问未授权资源" ));
            //}
            //else
            //{
            //    context.Succeed(requirement);
            //}

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
