using LargeProb.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LargeProb.Core.Controller
{
    [ApiController]
    //控制器/方法 策略
    //[Route("[controller]/[action]")]
    //RESTful风格
    [Route("[controller]")]
   
    public class ApiService : ControllerBase
    {
        #region 成功返回
        protected virtual SolutionResult Success()
        {
            return new SolutionResult(HttpStatusCode.OK, "响应成功", default);
        }
        protected virtual SolutionResult<T> Success<T>() where T : class, new()
        {
            return new SolutionResult<T>(HttpStatusCode.OK, "响应成功", default(T));
        }

        protected virtual SolutionResult Success(object data)
        {
            return new SolutionResult(HttpStatusCode.OK, "响应成功", data);
        }

        protected virtual SolutionResult<T> Success<T>(T data) where T : class, new()
        {
            return new SolutionResult<T>(HttpStatusCode.OK, "响应成功", data);
        }

        protected virtual SolutionResult Success(string msg, object? data)
        {
            return new SolutionResult(HttpStatusCode.OK, msg, data);
        }
        protected virtual SolutionResult<T> Success<T>(string msg, T? data) where T : class, new()
        {
            return new SolutionResult<T>(HttpStatusCode.OK, msg, data);
        }
        #endregion

        #region 错误返回
        protected virtual SolutionResult Error()
        {
            return new SolutionResult(HttpStatusCode.BadRequest, "响应失败", default);
        }
        protected virtual SolutionResult<T> Error<T>() where T : class, new()
        {
            return new SolutionResult<T>(HttpStatusCode.BadRequest, "响应失败", default(T));
        }

        protected virtual SolutionResult Error(string msg)
        {
            return new SolutionResult(HttpStatusCode.BadRequest, msg, default);
        }
        protected virtual SolutionResult<T> Error<T>(string msg) where T : class, new()
        {
            return new SolutionResult<T>(HttpStatusCode.BadRequest, msg, default(T));
        }

        protected virtual SolutionResult Error(string msg, object? data)
        {
            return new SolutionResult(HttpStatusCode.BadRequest, msg, data);
        }
        protected virtual SolutionResult<T> Error<T>(string msg, T? data) where T : class, new()
        {
            return new SolutionResult<T>(HttpStatusCode.BadRequest, msg, data);
        }

        #endregion

        /// <summary>
        /// 自定义返回
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual SolutionResult Result(HttpStatusCode code, string msg, object data)
        {
            return new SolutionResult(code, msg, data);
        }

        /// <summary>
        /// 自定义返回
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual SolutionResult<T> Result<T>(HttpStatusCode code, string msg, T data) where T : class, new()
        {
            return new SolutionResult<T>(code, msg, data);
        }
    }
}
