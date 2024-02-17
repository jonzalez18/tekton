using Sabio.Models.Domain.StripeSession;
using Sabio.Models.Requests.StripeRequest;
using Stripe;
using System.Collections.Generic;

namespace Sabio.Services.Interfaces
{
    public interface IFlowStripeService
    {
        int AddInvoice(StripeInvoiceAddRequest models, int userId);
        List<FlowCustomerSubscription> GetCustomerSubscriptions(int userId);
        public FlowCustomerSubscription GetCustomerSubscription(int userId);
    }
}