using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LargeProb.Core.Exceptions
{
    public class SolutionException : Exception
    {
        public override string Message => base.Message;

        public SolutionException(string? message) : base(message)
        {
        }

    }
}
