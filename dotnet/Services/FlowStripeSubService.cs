using Microsoft.Extensions.Options;
using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models.AppSettings;
using Sabio.Models.Domain.StripeSession;
using Sabio.Models.Requests.StripeRequest;
using Sabio.Services.Interfaces;
using Stripe;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Sabio.Services
{
    public class FlowStripeSubService : IFlowStripeService
    {
        IDataProvider _data = null;
        private readonly StripeConfig _config;
        public FlowStripeService(IDataProvider data, IOptions<StripeConfig> config)
        {
            _config = config.Value;
            _data = data;
        }
        public int AddInvoice(StripeInvoiceAddRequest models, int userId)
        {
            int id = 1;
            string procName = "[dbo].[StripePurchaseSubProd_Insert]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@UserId", userId);
                AddCommonParams(models, col);
                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;
                col.Add(idOut);

            }, returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;
                int.TryParse(oId.ToString(), out id);
            });
            return id;
        }
        private static void AddCommonParams(StripeInvoiceAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@TotalPrice", model.TotalPrice);
            col.AddWithValue("@StripeChargeId", model.StripeChargeId);
            col.AddWithValue("@StripeCustomerId", model.StripeCustomerId);
            col.AddWithValue("@StripeInvoiceId", model.StripeInvoiceId);
            col.AddWithValue("@StripeSubscriptionId", model.StripeSubscriptionId);
            col.AddWithValue("@DateEnded", model.DateEnded);
            col.AddWithValue("@isActive", model.IsActive);
            col.AddWithValue("@StripeProductId", model.StripeProductId);
            col.AddWithValue("@StripePriceId", model.StripePriceId);
            col.AddWithValue("@RecurringPeriod", model.RecurringCount);
            col.AddWithValue("@Created", model.Created);
            col.AddWithValue("@RecurringCount", model.RecurringCount);
            col.AddWithValue("@Name", model.Name);
            col.AddWithValue("@Images", model.Image);
        }
        public FlowCustomerSubscription GetCustomerSubscription(int userId)
        {
            string procName = "[dbo].[CustomerPurchaseSubscription_Select_ByUserId]";
            FlowCustomerSubscription customerSub = null;
            _data.ExecuteCmd(procName, delegate (SqlParameterCollection parameterCollection)
            {
                parameterCollection.AddWithValue("@UserId", userId);
            }, delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                customerSub = MapSingleCustomerSubscription(reader, ref startingIndex);
            });
            return customerSub;
        }
        public List<FlowCustomerSubscription> GetCustomerSubscriptions(int userId)
        {
            string procName = "[dbo].[CustomerPurchaseSubscription_SelectAll_ByUserId]";
            List<FlowCustomerSubscription> list = null;
            _data.ExecuteCmd(procName, delegate (SqlParameterCollection parameterCollection)
            {
                parameterCollection.AddWithValue("@UserId", userId);
            }, singleRecordMapper: delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                FlowCustomerSubscription customerSub = MapSingleCustomerSubscription(reader, ref startingIndex);
                if (list == null)
                {
                    list = new List<FlowCustomerSubscription>();
                }
                list.Add(customerSub);
            });
            return list;
        }
        private static FlowCustomerSubscription MapSingleCustomerSubscription(IDataReader reader, ref int startingIndex)
        {
            FlowCustomerSubscription customerSub = new FlowCustomerSubscription();
            FlowPrice price = new FlowPrice();
            FlowSubscription sub = new FlowSubscription();
            FlowPurchase purchase = new FlowPurchase();

            purchase.Id = reader.GetSafeInt32(startingIndex++);
            purchase.StripeChargeId = reader.GetSafeString(startingIndex++);
            purchase.StripeCustomerId = reader.GetSafeString(startingIndex++);
            purchase.StripeInvoiceId = reader.GetSafeString(startingIndex++);
            purchase.UserId = reader.GetSafeInt32(startingIndex++);
            customerSub.Purchase = purchase;

            sub.SubscriptionId = reader.GetSafeInt32(startingIndex++);
            sub.IsActive = reader.GetSafeString(startingIndex++);
            sub.StripeSubscriptionId = reader.GetSafeString(startingIndex++);
            customerSub.Subscription = sub;

            price.PriceId = reader.GetSafeInt32(startingIndex++);
            price.StripePriceId = reader.GetSafeString(startingIndex++);
            price.Name = reader.GetSafeString(startingIndex++);
            price.Image = reader.GetSafeString(startingIndex++);
            customerSub.Price = price;

            return customerSub;
        }
    }
}

