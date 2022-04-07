using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sample.Components.BatchConsumers;
using Sample.Components.Consumers;
using Sample.Components.StateMachines;
using Sample.Contracts;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace Sample.Service
{
    internal class program
    {
        static DependencyTrackingTelemetryModule _module;
        static TelemetryClient _telemetryClient;
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .CreateLogger();
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            var builder = new HostBuilder()
               .ConfigureServices((hostContext, services) =>
                {

                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance); 
                    var exchange = KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>();
                    services.AddMassTransit(mt =>
                    {
                        _module = new DependencyTrackingTelemetryModule();
                        _module.IncludeDiagnosticSourceActivities.Add("MassTransit");

                        TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
                        configuration.InstrumentationKey = "6b4c6c82-3250-4170-97d3-245ee1449278";
                        configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

                        _telemetryClient = new TelemetryClient(configuration);

                        _module.Initialize(configuration);

                        mt.UsingRabbitMq((context, configurator) =>
                        {
                            configurator.Host("amqp://lx:admin@8.142.71.127:5672/my_vhost2");
                            configurator.UseMessageScheduler(new Uri("queue:quartz"));

                            configurator.ReceiveEndpoint(KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(), e =>
                            {
                                e.PrefetchCount = 20;

                                e.Batch<RoutingSlipCompleted>(b =>
                                {
                                    b.MessageLimit = 10;
                                    b.TimeLimit = TimeSpan.FromSeconds(5);

                                    b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(context);
                                });
                            });

                            configurator.ConfigureEndpoints(context);


                        });
                        mt.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                        mt.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition)).RedisRepository(s=>s.DatabaseConfiguration("8.142.71.127:6379,password=123456"));

                        mt.AddRequestClient<CheckOrder>();
                    });

                    services.AddOptions<MassTransitHostOptions>()
                              .Configure(options =>
                              {
                                  options.WaitUntilStarted = true;
                              });
                }).ConfigureLogging((hostContext, logging) =>
                {
                    logging.AddSerilog(dispose: true);
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                });
            Console.WriteLine("saga running");
            builder.Build().Run();
            _telemetryClient?.Flush();
            _module?.Dispose();

            Log.CloseAndFlush();
        }
    }
}