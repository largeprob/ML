using LargeProb.Core.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LargeProb.Core.Authorization
{

    /// <summary>
    /// 一个身份授权响应的中间件
    /// </summary>
    public class GlobalAuthorResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();
        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            //说明中间有授权策略没通过
            if (authorizeResult.Forbidden && authorizeResult.AuthorizationFailure.FailureReasons.Any())
            {
                var firtErrorMsg = authorizeResult.AuthorizationFailure.FailureReasons.First().Message;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsJsonAsync(new SolutionResult(HttpStatusCode.BadRequest, firtErrorMsg));
            }
            // Fall back to the default implementation.
            await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
