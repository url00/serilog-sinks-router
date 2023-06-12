using System;
using Microsoft.Extensions.Options;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Router.Sinks.Router;

namespace Serilog.Sinks.Router
{
    public static class LoggerSinkConfigurationRouterSinkExtensions
    {
        public static LoggerConfiguration RouterSink(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            IOptionsMonitor<RouterSinkOptions> routerSinkOptionsMonitor,
            Action<LoggerSinkConfiguration> configureSinkA,
            Action<LoggerSinkConfiguration> configureSinkB)
        {
            ILogEventSink sinkA = null;
            ILogEventSink sinkB = null;
            LoggerSinkConfiguration.Wrap(
                new LoggerConfiguration().WriteTo,
                s => sinkA = s,
                configureSinkA,
                LevelAlias.Minimum,
                null);
            LoggerSinkConfiguration.Wrap(
                new LoggerConfiguration().WriteTo,
                s => sinkB = s,
                configureSinkB,
                LevelAlias.Minimum,
                null);


            var resultSink = new RouterSink(routerSinkOptionsMonitor, sinkA, sinkB);
            return loggerSinkConfiguration.Sink(resultSink);
        }
    }
}