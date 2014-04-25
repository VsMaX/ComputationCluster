using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Aspects;

namespace Communication_Library
{
    public class HandleExceptionAttribute : OnExceptionAspect
    {
        public override void OnException(MethodExecutionArgs args)
        {
            base.OnException(args);
        }
    }
}
