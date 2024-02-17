using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;
using Stripe;
using SessionService = Stripe.Checkout.SessionService;
using Session = Stripe.Checkout.Session;
using Sabio.Models.AppSettings;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace Sabio.Web.Api.Controllers.Stripe
{
    public class StripeSuccessController : BaseApiController
    {
        private readonly StripeConfig _config;
        public StripeSuccessController(IOptions<StripeConfig> config,
            ILogger<CheckoutApiController> logger) : base(logger)
        {
            _config = config.Value;
        }
        [HttpGet("/order/success")]
        public ActionResult OrderSuccess(string session_Id)
        {
            StripeConfiguration.ApiKey = _config.StripeApiKey;
            var code = 200;
            var sessionService = new SessionService();
            Session session = sessionService.Get(session_Id);
            var customerService = new CustomerService();
            Customer customer = customerService.Get(session.CustomerId);
            var invoiceOptions = new InvoiceListOptions
            {
                Limit = 1,
                Customer = customer.Id,
            };
            var invoiceService = new InvoiceService();
            StripeList<Invoice> invoices = invoiceService.List(
              invoiceOptions);
            return StatusCode(code, invoices);
        }
        [HttpGet("/v2/product")]
        public ActionResult<ItemResponse<Product>> GetProduct([FromQuery(Name = "prodId")] string prodId)
        {
            StripeConfiguration.ApiKey = _config.StripeApiKey;
            int code = 200;

            var service = new ProductService();
            var product = service.Get(prodId);
            return StatusCode(code, product);
        }

        [HttpGet("/subscription")]
        public ActionResult<ItemResponse<Subscription>> Get([FromQuery(Name = "id")] string id)
        {
            StripeConfiguration.ApiKey = _config.StripeApiKey;
            int code = 200;
            var service = new SubscriptionService();
            var sub = service.Get(id);
            return StatusCode(code, sub);
        }
        [HttpGet("/v1/products")]
        [AllowAnonymous]
        public ActionResult<StripeList<Product>> GetProducts()
        {
            int code = 200;
            StripeConfiguration.ApiKey = _config.StripeApiKey;
            var options = new ProductListOptions
            {
                Limit = 3
            };
            var service = new ProductService();
            StripeList<Product> products = service.List(
              options);
            return StatusCode(code, products);
        }
    }
}