using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LargeProb.Core.Controller
{
    [Serializable]
    public class SolutionResult
    {
        /// <summary>
        /// 响应码
        /// </summary>
        [Description("响应码")]
        public HttpStatusCode Code { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public object? Data { get; set; }
        public SolutionResult(HttpStatusCode code, string msg, object? data = default)
        {
            Code = code;
            Msg = msg;
            Data = data;
        }
    }


    [Serializable]
    public class SolutionResult<T>
    {
        /// <summary>
        /// 响应码
        /// </summary>
        [Description("响应码")]
        public HttpStatusCode Code { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        [Description("信息")]
        public string Msg { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [Description("数据")]
        public T? Data { get; set; }

        public SolutionResult(HttpStatusCode code, string msg, T? data = default)
        {
            Code = code;
            Msg = msg;
            Data = data;
        }
    }
}
