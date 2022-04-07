using MassTransit;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components
{
    public class ContainerScopedFilter :
       IFilter<ConsumeContext<SubmitOrder>>
    {
        public Task Send(ConsumeContext<SubmitOrder> context, IPipe<ConsumeContext<SubmitOrder>> next)
        {
            var provider = context.GetPayload<IServiceProvider>();

            Console.WriteLine("Filter ran");

            return next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}
