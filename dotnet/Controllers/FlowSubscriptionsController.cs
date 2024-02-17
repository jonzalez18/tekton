using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sabio.Models.AppSettings;
using Sabio.Models.Domain.StripeSession;
using Sabio.Models.Requests.StripeRequest;
using Sabio.Services;
using Sabio.Services.Interfaces;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;
using Stripe;
using System;
using System.Collections.Generic;

namespace Sabio.Web.Api.Controllers.Stripe
{
    [Route("/stripe")]
    [ApiController]
    public class FlowSubscriptionsController : BaseApiController
    {
        private IFlowStripeService _flowStripeService;
        private IAuthenticationService<int> _authService;
        private readonly StripeConfig _config;
        public FlowStripeController(IFlowStripeService flowStripeService
            , IAuthenticationService<int> authService
            , ILogger<FlowStripeController> logger
            , IOptions<StripeConfig> config) : base(logger)
        {
            _flowStripeService = flowStripeService;
            _authService = authService;
            _config = config.Value;
        }
        [HttpPost]
        public ActionResult<ItemResponse<int>> Create(StripeInvoiceAddRequest model)
        {
            BaseResponse response = null;
            int userId = _authService.GetCurrentUserId();
            int code = 201;
            try
            {
                int id = _flowStripeService.AddInvoice(model, userId);
                if (id == 0)
                {
                    code = 500;
                    response = new ErrorResponse("Generic Message: Invoice not Found.");
                }
                else
                {
                    response = new ItemResponse<int>() { Item = id };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Message: {ex.Message}");
                code = 500;
            }
            return StatusCode(code, response);
        }
        [HttpGet]
        public ActionResult<ItemResponse<FlowCustomerSubscription>> GetById()
        {
            BaseResponse response = null;
            int userId = _authService.GetCurrentUserId();
            ActionResult result = null;
            try
            {
                FlowCustomerSubscription customerSub = _flowStripeService.GetCustomerSubscription(userId);
                response = new ItemResponse<FlowCustomerSubscription>();
                if (customerSub == null)
                {
                    result = Ok200(null);
                }
                else
                {
                    response = new ItemResponse<FlowCustomerSubscription> { Item = customerSub };
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }
        [HttpGet("customer/subscriptions")]
        public ActionResult<ItemsResponse<FlowCustomerSubscription>> GetAllById()
        {
            int userId = _authService.GetCurrentUserId();
            BaseResponse response = null;
            ActionResult result = null;
            try
            {
                response = new ItemsResponse<FlowCustomerSubscription>();
                List<FlowCustomerSubscription> list = _flowStripeService.GetCustomerSubscriptions(userId);
                if (list == null)
                {
                    result = Ok200(null);
                }
                else
                {
                    response = new ItemsResponse<FlowCustomerSubscription> { Items = list };
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }
    }
}
