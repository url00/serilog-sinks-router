# Router Sink for Serilog

Uses DynamicExpresso and IOptions tracking to enable dynamic routing of LogEvents between two different sinks.

[![NuGet version (Serilog.Sinks.Router)](https://img.shields.io/nuget/v/Serilog.Sinks.Router?style=flat-square)](https://www.nuget.org/packages/Serilog.Sinks.Router/)

## Targets

* [.NET Standard 2.1](https://github.com/dotnet/standard/blob/master/docs/versions.md) (netstandard2.1)

## Usage

From inside a ASP.Net Core application:

```csharp
var builder = WebApplication.CreateBuilder(args);

// To enable dynamic recompilation you must enable the "reloadOnChange" flag.
builder.Configuration.AddJsonFile("logging.json", optional: false, reloadOnChange: true);

builder.Services.AddOptions();
builder.Services.Configure<RouterSinkOptions>(builder.Configuration.GetSection("RouterSink"));

// This example uses the ApplicationInsights sink as the A sink.
builder.Services.AddApplicationInsightsTelemetry();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.RouterSink(services.GetRequiredService<IOptionsMonitor<RouterSinkOptions>>(), sinkAConf => sinkAConf.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces), sinkBConf => sinkBConf.Console());
});
```


#### Configuration

The current `ILogEvent` is made implicitly available by the configured DynamicExpresso interpreter.
So accessing the `Properties` of the `ILogEvent` is as simple as using `Properties[keyForPropertyYouWant]`.

##### Simple example based on event level:

This configuration only uses the A sink if the log event level is debug, and everything else will get logged by the B sink.

```json
{
    "RouterSink": {
        "ShouldEmitSinkAExpression": "Level == LogEventLevel.Debug",
        "ShouldEmitSinkBExpression": ""
    }
}
```

##### More complex example based on request host information:

Now the A sink only logs if the port number from the request host information contains the port 7443.
The B sink is currently disabled, but that can be changed dynamically at runtime without restarting the server.

```json
{
    "RouterSink": {
        "ShouldEmitSinkAExpression": "Properties.Keys.Contains(\"RequestPath\") && Properties.Keys.Contains(\"Host\") && Properties[\"Host\"].ToString().Contains(\":7443\")",
        "ShouldEmitSinkBExpression": "false"
    }
}
```


## License

MIT


