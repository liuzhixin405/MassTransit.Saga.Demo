using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sample.Components.Consumers;
using Sample.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();

builder.Services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
var exchange = KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>();
builder.Services.AddMassTransit(mt =>
{
    mt.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("amqp://lx:admin@8.142.71.127:5672/my_vhost2");

    });
   
    mt.AddRequestClient<SubmitOrder>(new Uri($"exchange:{exchange}"));   //必须是广播模式fanout，手动建。否则自动直连模式消费不了数据，也返回不了数据
    mt.AddRequestClient<CheckOrder>();
    
});
builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = check => check.Tags.Contains("ready");
});
builder.Services.AddOptions<MassTransitHostOptions>() 
      .Configure(options =>
      {
          options.WaitUntilStarted = true;
      });
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
   endpoints.MapControllers();

    // The readiness check uses all registered checks with the 'ready' tag.
    endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
    });

    endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        // Exclude all checks and return a 200-Ok.
        Predicate = _ => false
    });
});
//app.MapControllers();

app.Run();
