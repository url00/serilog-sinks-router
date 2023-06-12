﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DynamicExpresso;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Sinks.Router.Sinks.Router
{
    public class RouterSink : ILogEventSink, IDisposable
    {
        private readonly ILogEventSink sinkA;
        private readonly ILogEventSink sinkB;
        private readonly IDisposable? routerSinkOptionsMonitorDisposable;
        private readonly Interpreter interpreter;


        private RouterSinkOptions currentOptions = new RouterSinkOptions();


        private Func<LogEvent, bool> compiledShouldEmitAFunc;
        private string shouldEmitAExpression = "false";
        private Func<LogEvent, bool> compiledShouldEmitBFunc;
        private string shouldEmitBExpression = "false";


        public RouterSink(IOptionsMonitor<RouterSinkOptions> routerSinkOptionsMonitor, ILogEventSink sinkA, ILogEventSink sinkB)
        {
            this.sinkA = sinkA ?? throw new ArgumentNullException(nameof(sinkA));
            this.sinkB = sinkB ?? throw new ArgumentNullException(nameof(sinkB));

            interpreter = new Interpreter();
            interpreter.Reference(typeof(LogEventLevel));

            compiledShouldEmitAFunc = interpreter.ParseAsDelegate<Func<LogEvent, bool>>(shouldEmitAExpression);
            compiledShouldEmitBFunc = interpreter.ParseAsDelegate<Func<LogEvent, bool>>(shouldEmitBExpression);

            routerSinkOptionsMonitorDisposable = routerSinkOptionsMonitor.OnChange(Reconfigure);
            Reconfigure(routerSinkOptionsMonitor.CurrentValue);
        }


        private void Reconfigure(RouterSinkOptions options)
        {
            if (string.IsNullOrEmpty(options.ShouldEmitSinkAExpression))
            {
                options.ShouldEmitSinkAExpression = "false";
            }

            if (string.IsNullOrEmpty(options.ShouldEmitSinkBExpression))
            {
                options.ShouldEmitSinkBExpression = "false";
            }

            currentOptions = options;

            try
            {
                var possibleNewEmit = interpreter.ParseAsDelegate<Func<LogEvent, bool>>(options.ShouldEmitSinkAExpression, "this");
                compiledShouldEmitAFunc = possibleNewEmit;
                LogString(sinkA, shouldEmitBExpression);
            }
            catch (Exception e)
            {
                SelfLog.WriteLine("Error parsing expression: {0} {1} {2}", nameof(compiledShouldEmitAFunc), options.ShouldEmitSinkAExpression, e);
            }

            try
            {
                var possibleNewEmit = interpreter.ParseAsDelegate<Func<LogEvent, bool>>(options.ShouldEmitSinkBExpression, "this");
                compiledShouldEmitBFunc = possibleNewEmit;
                LogString(sinkB, shouldEmitBExpression);
            }
            catch (Exception e)
            {
                SelfLog.WriteLine("Error parsing expression: {0} {1} {2}", nameof(compiledShouldEmitAFunc), options.ShouldEmitSinkBExpression, e);
            }
        }



        private static void LogString(ILogEventSink sink, string s)
        {
            var token = new TextToken(s);
            sink.Emit(new LogEvent(
                DateTimeOffset.UtcNow,
                LogEventLevel.Debug,
                null,
                new MessageTemplate(new[] { token }),
                new List<LogEventProperty>()));
        }



        public void Emit(LogEvent logEvent)
        {


            var shouldEmitAResult = false;
            try
            {
                shouldEmitAResult = compiledShouldEmitAFunc(logEvent);
            }
            catch (Exception e)
            {
                shouldEmitAResult = true;
                SelfLog.WriteLine("Error evaluating expression: {0} {1} {2}", nameof(compiledShouldEmitAFunc), shouldEmitAExpression, e);
            }
            if (shouldEmitAResult)
            {
                sinkA.Emit(logEvent);
            }



            var shouldEmitBResult = false;
            try
            {
                shouldEmitBResult = compiledShouldEmitBFunc(logEvent);
            }
            catch (Exception e)
            {
                shouldEmitBResult = true;
                SelfLog.WriteLine("Error evaluating expression: {0} {1} {2}", nameof(compiledShouldEmitBFunc), shouldEmitBExpression, e);
            }
            if (shouldEmitBResult)
            {
                sinkB.Emit(logEvent);
            }
        }



        public void Dispose()
        {
            (sinkA as IDisposable)?.Dispose();
            (sinkB as IDisposable)?.Dispose();
            routerSinkOptionsMonitorDisposable?.Dispose();
        }
    }



}
