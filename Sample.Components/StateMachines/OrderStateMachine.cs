﻿using MassTransit;
using Sample.Components.StateMachines.OrderStateMachineActivities;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Components.StateMachines
{
    public class OrderStateMachine:MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => OrderSubmitted, x => x.CorrelateById(a => a.Message.OrderId));
            Event(() => OrderAccepted, x => x.CorrelateById(a => a.Message.OrderId));
            Event(() => OrderStatusRequested, x => {
                x.CorrelateById(a => a.Message.OrderId);
                x.OnMissingInstance(a => a.ExecuteAsync(async context =>
                  {
                      if (context.RequestId.HasValue)
                      {
                          await context.RespondAsync<OrderNotFound>(new { context.Message.OrderId});
                      }
                  }));
            }) ;

            Event(() => AccountClosed, x => x.CorrelateBy((instance,context) => instance.CustomerNumber==context.Message.CustomerNumber));

            InstanceState(x => x.CurrentState);

            Initially(
                When(OrderSubmitted)
                .Then(context =>
                {
                    context.Instance.SubmitDate = context.Data.Timestamp;
                    context.Instance.CustomerNumber = context.Data.CustomerNumber;
                    context.Instance.Updated = DateTime.UtcNow;
                }).TransitionTo(Submitted));

            During(Submitted, Ignore(OrderSubmitted),When(AccountClosed).TransitionTo(Canceled),
                When(OrderAccepted).Activity(x=>x.OfType<AcceptOrderActivity>()).TransitionTo(Accepted));

            DuringAny(When(OrderStatusRequested).RespondAsync(x => x.Init<OrderStatus>(new
            {
                OrderId = x.Instance.CorrelationId,
                State = x.Instance.CurrentState
            })));
            DuringAny(When(OrderSubmitted).Then(context =>
            {
                context.Instance.SubmitDate = context.Data.Timestamp;
                context.Instance.CustomerNumber ??= context.Data.CustomerNumber;
            }));
        }

        public State Submitted { get; private set; }
        public State Accepted { get;private set; }
        public State Canceled { get; private set; }

        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<OrderAccepted> OrderAccepted { get; private set; }
        public Event<CustomerAccountClosed> AccountClosed { get; private set; }
        public Event<CheckOrder> OrderStatusRequested { get; private set; }

    }
}
