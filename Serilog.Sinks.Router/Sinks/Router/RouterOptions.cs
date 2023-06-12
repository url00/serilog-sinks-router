using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.Router.Sinks.Router
{
    public class RouterSinkOptions
    {
        public string ShouldEmitSinkAExpression { get; set; }
        public string ShouldEmitSinkBExpression { get; set; }
    }
}
