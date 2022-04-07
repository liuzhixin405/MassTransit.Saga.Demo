using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    public interface OrderSubmissionRejected
    {
        public Guid OrderId { get; }
        public DateTime Timestamp { get; }
        public string CustomerNumber { get; }
        public string Reason { get; }
    }
}
