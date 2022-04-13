using Automatonymous;
using MassTransit;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.StateMachines.OrderStateMachineActivities
{
    public class AcceptOrderActivity : IStateMachineActivity<OrderState, OrderAccepted>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, OrderAccepted> context, IBehavior<OrderState, OrderAccepted> next)
        {
            Console.WriteLine($"****************** order is {context.Message.OrderId}  ****************");
           await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, OrderAccepted, TException> context, IBehavior<OrderState, OrderAccepted> next) where TException : Exception
        {
           return next.Faulted(context);    
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("accept-order");
        }
    }
}
