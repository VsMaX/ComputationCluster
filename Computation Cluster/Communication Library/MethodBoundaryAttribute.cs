using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Aspects;

namespace Communication_Library
{
    [Serializable]
    public class MethodBoundaryAttribute : OnMethodBoundaryAspect
    {
        private static readonly log4net.ILog _logger
            = log4net.LogManager.GetLogger(
                    System.Reflection.MethodBase.GetCurrentMethod()
                     .DeclaringType);

        public override void OnEntry(MethodExecutionArgs args)
        {
            _logger.Debug("Entering method: " + args.Method.Name);
            base.OnEntry(args);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            _logger.Debug("Exiting method: " + args.Method.Name);
            base.OnExit(args);
        }

        public override void OnException(MethodExecutionArgs args)
        {
            args.FlowBehavior = FlowBehavior.Continue;
            _logger.Fatal(args.Exception.ToString());
            base.OnException(args);
        }
    }
}
