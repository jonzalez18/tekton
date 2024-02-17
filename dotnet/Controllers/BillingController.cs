using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sabio.Models.AppSettings;
using Sabio.Web.Controllers;
using Stripe;
using Stripe.BillingPortal;


namespace Sabio.Web.Api.Controllers.Stripe
{
    [Route("/billing/session")]
    [ApiController]
    public class FlowCustomerPortal : BaseApiController
    {
        private readonly StripeConfig _config;
        private readonly Domain _domainConfig;
        public FlowCustomerPortal(IOptions<StripeConfig> config, IOptions<Domain> domainConfig,
            ILogger<FlowCustomerPortal> logger) : base(logger)
        {
            _config = config.Value;
            _domainConfig = domainConfig.Value;
        }

        [HttpPost]
        public ActionResult CustomerPortal([FromQuery(Name = "customerId")] string customerId)
        {
            StripeConfiguration.ApiKey = _config.StripeApiKey;
            int code = 200;

            var options = new SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = _domainConfig.DomainName,
            };
            var service = new SessionService();
            var session = service.Create(options);
            return StatusCode(code, session);
        }
    }
}