using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sabio.Models.Requests.StripeRequest;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using Sabio.Services;
using System;
using System.Collections.Generic;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using SessionService = Stripe.Checkout.SessionService;
using Session = Stripe.Checkout.Session;
using Sabio.Models.AppSettings;
using Microsoft.Extensions.Options;
using Sabio.Models.Domain.StripeSession;

namespace Sabio.Web.Api.Controllers.Stripe
{
    [Route("/checkout/session")]
    [ApiController]
    public class CheckoutApiController : BaseApiController
    {
        private readonly StripeConfig _config;
        private readonly Domain _domainConfig;
        public CheckoutApiController(IOptions<StripeConfig> config,
            IOptions<Domain> domainConfig,
            ILogger<CheckoutApiController> logger) : base(logger)
        {
            _config = config.Value;
            _domainConfig = domainConfig.Value;
        }
        [HttpPost]
        public ActionResult Create([FromQuery(Name = "customerId")] string customerId, StripePriceId priceId)
        {
            StripeConfiguration.ApiKey = _config.StripeApiKey;
            var domain = _domainConfig.DomainName;
            var code = 200;
            SessionCreateOptions options = null;
            if (customerId != null)
            {
                options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    Price = priceId.PriceId,
                    Quantity = 1,
                  },
                },
                    Customer = customerId,
                    Mode = "subscription",
                    SuccessUrl = domain + "/checkout/success?session={CHECKOUT_SESSION_ID}",
                    CancelUrl = domain + "?canceled=true"
                };
            }
            else
            {
                options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    Price = priceId.PriceId,
                    Quantity = 1,
                  },
                },
                    Mode = "subscription",
                    SuccessUrl = domain + "/checkout/success?session={CHECKOUT_SESSION_ID}",
                    CancelUrl = domain + "?canceled=true"
                };
            }
            var service = new SessionService();
            Session session = service.Create(options);
            Response.Headers.Add("Location", session.Url);
            return StatusCode(code, session);
        }
    }
}