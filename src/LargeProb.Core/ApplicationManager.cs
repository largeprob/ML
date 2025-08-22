using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LargeProb.Core
{
    public class ApplicationManager
    {
        /// <summary>
        /// 服务
        /// </summary>
        public static IEnumerable<Type> ServiceTypes = new List<Type>();

        /// <summary>
        /// 实体
        /// </summary>
        public static IEnumerable<Type> EntryTypes = new List<Type>();
    }
}
