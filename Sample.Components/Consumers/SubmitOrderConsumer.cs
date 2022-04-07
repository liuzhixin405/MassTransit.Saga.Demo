﻿using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.Consumers
{
    public class SubmitOrderConsumer:IConsumer<SubmitOrder>
    {
        readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger?.Log(LogLevel.Debug, "SubmitOrderConsumer: {CustomerNumber}", context.Message.CustomerNumber);
            if (context.Message.CustomerNumber.Contains("TEST"))
            {
                if (context.RequestId != null)
                {
                    await context.RespondAsync<OrderSubmissionRejected>(new
                    {
                        InVar.Timestamp,
                        context.Message.OrderId,
                        context.Message.CustomerNumber,
                        Reason = $"Test Customer cannot submit orders: {context.Message.CustomerNumber}"
                    });

                    return;
                }
            }

                MessageData<string> notes = context.Message.Notes;
                if (notes?.HasValue ?? false)
                {
                    string notesValue = await notes.Value;

                    Console.WriteLine("NOTES: {0}", notesValue);
                }

                await context.Publish<OrderSubmitted>(new
                {
                    context.Message.OrderId,
                    context.Message.Timestamp,
                    context.Message.CustomerNumber,
                    context.Message.PaymentCardNumber,
                    context.Message.Notes
                }); //saga

                if (context.RequestId != null)
                    await context.RespondAsync<OrderSubmissionAccepted>(new
                    {
                        InVar.Timestamp,
                        context.Message.OrderId,
                        context.Message.CustomerNumber
                    });
            }
    }
}
