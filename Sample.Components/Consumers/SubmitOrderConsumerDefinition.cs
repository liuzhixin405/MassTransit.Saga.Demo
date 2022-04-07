using MassTransit;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Components.Consumers
{
    public class SubmitOrderConsumerDefinition:
      ConsumerDefinition<SubmitOrderConsumer>
    {
        readonly IServiceProvider _serviceProvider;

    public SubmitOrderConsumerDefinition(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        ConcurrentMessageLimit = 20;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(3, 1000));
        endpointConfigurator.UseServiceScope(_serviceProvider);

        consumerConfigurator.Message<SubmitOrder>(m => m.UseFilter(new ContainerScopedFilter()));
    }
}
}
