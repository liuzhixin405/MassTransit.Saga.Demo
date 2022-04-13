using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    public interface OrderSubmitted
    {
       public Guid OrderId { get; }
        public DateTime Timestamp { get; }

        public string CustomerNumber { get; }
        public string PaymentCardNumber { get; }

        public MessageData<string> Notes { get; }
    }

    public interface CustomerAccountClosed
    {
        Guid CustomerId { get; set; }
        public string CustomerNumber { get; }
    }
}
