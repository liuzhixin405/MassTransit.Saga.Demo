using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sample.Contracts;

namespace Simple.Order.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IPublishEndpoint endpoint;
        public CustomerController(IPublishEndpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id,string customerNumber)
        {
            await endpoint.Publish<CustomerAccountClosed>(new
            {
                CustomerId = id,
                CustomerNumber = customerNumber
            });
            return Ok();
        }
    }
}
