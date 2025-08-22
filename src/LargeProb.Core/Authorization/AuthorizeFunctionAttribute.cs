using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LargeProb.Core.Authorization
{

    /// <summary>
    /// 标识接口归属的功能点，按功能点授权分配权限
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [DebuggerDisplay("{ToString(),nq}")]
    public class AuthorizeFunctionAttribute: Attribute
    {
        public string[] Funcs { get; }

        public AuthorizeFunctionAttribute(params string[] funcs)
        {
            Funcs = funcs;
        }

        public override string? ToString()
        {
            return string.Join(", ", Funcs);
        }
    }
}
