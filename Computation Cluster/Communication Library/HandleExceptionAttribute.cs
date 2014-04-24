using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Aspects;

namespace Communication_Library
{
    [Serializable]
    public class HandleExceptionAttribute : OnExceptionAspect
    {
        public override void OnException(MethodExecutionArgs args)
        {
            args.FlowBehavior = FlowBehavior.Continue;
            //TODO logging
            Trace.WriteLine(args.Exception.ToString());
            base.OnException(args);
        }
    }
}
