using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PayPal;
using PayPal.Api;
using PayPal.PayPalAPIInterfaceService;
using PayPal.PayPalAPIInterfaceService.Model;
using zCush.Services.Shipping;

namespace zCush.Partners.PayPal
{
    public class PayPalService
    {
        //public Sale GetPayPalOrders()
        //{
        //    //var saleId = "14X52113A9302202F";
        //    // ### Api Context
        //    // Pass in a `APIContext` object to authenticate 
        //    // the call and to send a unique request id 
        //    // (that ensures idempotency). The SDK generates
        //    // a request id if you do not pass one explicitly. 
        //    // See [Configuration.cs](/Source/Configuration.html) to know more about APIContext.
        //    var apiContext = PayPalConfiguration.GetAPIContext();

        //    var saleId = "4V7971043K262623A";

        //    // ^ Ignore workflow code segment
        //    #region Track Workflow
        //    //this.flow.AddNewRequest("Get sale", description: "ID: " + saleId);
        //    #endregion

        //    var sale = Sale.Get(apiContext, saleId);

        //    return sale;
        //}

        public Plan GetBillingPlan()
        {

             var planId = "P-5FY40070P6526045UHFWUVEI";

            #region Track Workflow
            //--------------------
            //this.flow.AddNewRequest(title: "Get billing plan", description: "ID: " + planId);
            //--------------------
            #endregion

             var plan = Plan.Get(PayPalConfiguration.GetAPIContext(), planId);

            return plan;
        }

        public Agreement GetAgreement()
        {
            var apiContext = PayPalConfiguration.GetAPIContext();

            // ### Retrieve the Billing Agreement
            // The billing agreement being retrieved is one that was previously created and executed using a PayPal account as the funding source.
            //var agreementId = "I-6GW9AAE865PK";
            var agreementId = "I-UWM998D05VB3";

            #region Track Workflow
            //--------------------
            //this.flow.AddNewRequest(title: "Get billing agreement", description: "ID: " + agreementId);
            //--------------------
            #endregion

            // Use `Agreement.Get()` to retrieve the billing agreement details.
            var agreement = Agreement.Get(apiContext, agreementId);

            return agreement;
        }

        public List<PurchaseOrder> GetPayPalOrders()
        {
            var PPPos = new List<PurchaseOrder>();
            var payPalTransactions = TransactionSearchAPIOperation().PaymentTransactions.Where(pt => pt.Status == "Completed");
            var products = Products.GetAllProducts().ToDictionary(k => k.Description, v => v.SKU);
            foreach(var ppt in payPalTransactions)
            {
                var pptDetails = GetTransactionDetails(ppt.TransactionID);
                var shipToAddress = GetAddress(pptDetails.PaymentTransactionDetails.PayerInfo.Address);
                var purchaseOrder = new PurchaseOrder
                {
                    PONumber = ppt.TransactionID,
                    ShipAddress = shipToAddress
                };
               
                foreach(var paymentItem in  pptDetails.PaymentTransactionDetails.PaymentItemInfo.PaymentItem)
                {
                    if (paymentItem.Name != null &&
                       paymentItem.Amount.value != null &&
                       paymentItem.Quantity != null)
                    {
                        purchaseOrder.POLineItems.Add(new POLineItem
                        {
                            ProductDescription = paymentItem.Name,
                            Price = decimal.Parse(paymentItem.Amount.value),
                            Quantity = int.Parse(paymentItem.Quantity)
                        });
                    }                    
                }

                var ss = new ShippingService();
                ss.CreateFedExLabel(shipToAddress, Shipping3PartyAccounts.None, ppt.TransactionID);

                if (purchaseOrder.POLineItems.Any())
                {
                    PPPos.Add(purchaseOrder);
                }
            }

            return PPPos;
        }

        // # TransactionSearch API Operation
        // The TransactionSearch API searches transaction history for transactions that meet the specified criteria
        public TransactionSearchResponseType TransactionSearchAPIOperation()
        {
            // Create the TransactionSearchResponseType object
            var responseTransactionSearchResponseType = new TransactionSearchResponseType();

            try
            {
                // # Create the TransactionSearchReq object
                var requestTransactionSearch = new TransactionSearchReq();

                // `TransactionSearchRequestType` which takes mandatory argument:
                // 
                // * `Start Date` - The earliest transaction date at which to start the
                // search.
                var transactionSearchRequest = new TransactionSearchRequestType("2014-12-29T00:00:00+0530");
                requestTransactionSearch.TransactionSearchRequest = transactionSearchRequest;

                // Create the service wrapper object to make the API call
                var service = new PayPalAPIInterfaceServiceService(PayPalConfiguration.GetConfig());

                // # API call
                // Invoke the TransactionSearch method in service wrapper object
                responseTransactionSearchResponseType = service.TransactionSearch(requestTransactionSearch);

                if (responseTransactionSearchResponseType != null)
                {
                    // Response envelope acknowledgement
                    string acknowledgement = "TransactionSearch API Operation - ";
                    acknowledgement += responseTransactionSearchResponseType.Ack.ToString();

                    // # Success values
                    if (responseTransactionSearchResponseType.Ack.ToString().Trim().ToUpper().Equals("SUCCESS"))
                    {
                        // Search Results
                        IEnumerator<PaymentTransactionSearchResultType> iterator = responseTransactionSearchResponseType.PaymentTransactions.GetEnumerator();

                        while (iterator.MoveNext())
                        {
                            PaymentTransactionSearchResultType searchResult = iterator.Current;
                            Console.WriteLine("Transaction ID : " + searchResult.TransactionID + "\n");
                        }
                    }
                    // # Error Values
                    else
                    {
                        List<ErrorType> errorMessages = responseTransactionSearchResponseType.Errors;
                        foreach (ErrorType error in errorMessages)
                        {
                            Console.WriteLine("API Error Message : " + error.LongMessage + "\n");
                        }
                    }
                }
            }
            // # Exception log    
            catch (System.Exception ex)
            {
                Console.WriteLine("Error Message : " + ex.Message);
            }
            return responseTransactionSearchResponseType;
        }

        // # GetTransactionDetails API Operation
        // The GetTransactionDetails API operation obtains information about a specific transaction. 
        public GetTransactionDetailsResponseType GetTransactionDetails(string transactionId)
        {
            // Create the GetTransactionDetailsResponseType object
            var responseGetTransactionDetailsResponseType = new GetTransactionDetailsResponseType();

            try
            {
                // Create the GetTransactionDetailsReq object
                var getTransactionDetails = new GetTransactionDetailsReq();
                var getTransactionDetailsRequest = new GetTransactionDetailsRequestType();

                // Unique identifier of a transaction.
                // `Note:
                // The details for some kinds of transactions cannot be retrieved with
                // GetTransactionDetails. You cannot obtain details of bank transfer
                // withdrawals, for example.`
                getTransactionDetailsRequest.TransactionID = transactionId;
                getTransactionDetails.GetTransactionDetailsRequest = getTransactionDetailsRequest;

                // Create the service wrapper object to make the API call
                var service = new PayPalAPIInterfaceServiceService(PayPalConfiguration.GetConfig());

                // # API call
                // Invoke the GetTransactionDetails method in service wrapper object
                responseGetTransactionDetailsResponseType = service.GetTransactionDetails(getTransactionDetails);

                if (responseGetTransactionDetailsResponseType != null)
                {
                    // Response envelope acknowledgement
                    string acknowledgement = "GetTransactionDetails API Operation - ";
                    acknowledgement += responseGetTransactionDetailsResponseType.Ack.ToString();
                    Console.WriteLine(acknowledgement + "\n");

                    // # Success values
                    if (responseGetTransactionDetailsResponseType.Ack.ToString().Trim().ToUpper().Equals("SUCCESS"))
                    {
                        Console.WriteLine("Payer ID : " + responseGetTransactionDetailsResponseType.PaymentTransactionDetails.PayerInfo.PayerID + "\n");

                    }
                    // # Error Values             
                    else
                    {
                        List<ErrorType> errorMessages = responseGetTransactionDetailsResponseType.Errors;
                        foreach (ErrorType error in errorMessages)
                        {
                            Console.WriteLine("API Error Message : " + error.LongMessage + "\n");
                        }
                    }
                }
            }
            // # Exception log    
            catch (System.Exception ex)
            {
                // Log the exception message       
                Console.WriteLine("Error Message : " + ex.Message);
            }
            return responseGetTransactionDetailsResponseType;
        }

        public Payment GetPayment()
        {
            var apiContext = PayPalConfiguration.GetAPIContext();
            return Payment.Get(apiContext, "31038677X43877236");            
        }

        public PaymentHistory GetPayments()
        {
            var apiContext = PayPalConfiguration.GetAPIContext();
            var startDate = "2013-12-24";
            var endDate = "2015-12-27";
            return Payment.List(apiContext:apiContext, startDate:startDate, endDate:endDate);
        }

        public PlanList GetPlans()
        {
            var apiContext = PayPalConfiguration.GetAPIContext();
            var startDate = "2013-12-24";
            var endDate = "2015-12-27";
            return Plan.List(apiContext: apiContext, status: "CREATED");
        }

        private zCush.Common.Dtos.Address GetAddress(AddressType address)
        {
            return new Common.Dtos.Address
            {
                ContactName = address.Name,
                AddressLine1 = address.Street1,
                AddressLine2 = address.Street2,
                City = address.CityName,
                State = address.StateOrProvince,
                ZipCode = address.PostalCode
            };
        }
    }
}