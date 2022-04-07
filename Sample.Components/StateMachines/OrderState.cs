using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Components.StateMachines
{
    public class OrderState : SagaStateMachineInstance, ISagaVersion
    {
        public int Version { get ; set ; }
        public Guid CorrelationId { get ; set; }
        public string CurrentState { get; set; }
        public DateTime Updated { get; set; }
        public DateTime SubmitDate { get; set; }
        public string CustomerNumber { get; set; }
    }
}
