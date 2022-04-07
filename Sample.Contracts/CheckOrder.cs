using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    public interface CheckOrder
    {
       public Guid OrderId { get; set; }
    }

    public interface OrderStatus
    {
        public Guid OrderId { get; set; }
        public string State { get; set; }
    }

    public interface OrderNotFound
    {
        public Guid OrderId { get; }
    }
}
