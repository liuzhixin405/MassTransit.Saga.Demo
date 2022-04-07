using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    public interface OrderSubmissionAccepted
    {
      public  Guid OrderId { get; }
      public  DateTime Timestamp { get; }
       public string CustomerNumber { get; }
    }
}
