using System;
using MassTransit;

namespace Sample.Contracts
{
    public interface SubmitOrder
    {
        public Guid OrderId { get; }
        public DateTime Timestamp { get; }
        public string CustomerNumber { get; }
        public string PaymentCardNumber { get; }

        public MessageData<string> Notes { get; }
    }
}
