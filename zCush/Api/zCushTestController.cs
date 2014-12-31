using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Http;
using zCush.Common.Dtos;
using zCush.Orders.Print;
using zCush.Partners.PayPal;
using zCush.Services.Shipping;
using PayPal.Api;
using PayPal.PayPalAPIInterfaceService.Model;

namespace zCush.Api
{
    public class zCushTestController : ApiController
    {
        public string Get()
        {
            return "Hello world";
        }

        [HttpGet]
        public HttpResponseMessage TestPrinting()
        {
            var psl = new PrintShippingLabel();
            //psl.SendToPrinter();
            //psl.PrintPDFs("C:/test.pdf", "HPPrinter");
            //psl.PrintUsingName(); //This Works 
            //psl.TestPrintWithInterop(); 
            //psl.OneMorePrintTry();
            //psl.PrintKarlayBhai(@"C:/test.pdf", "HPPrinter");
            //Process.Start(@"LPR -S -P raw C:\test.pdf");

            var impersonationUsername = "zCushServiceAccount";
            var impersonationPassword = "godri786";
            var impersonationDomain = "localhost";
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            process.StartInfo.WorkingDirectory = "c:\\";
            startInfo.UseShellExecute = false;
            //startInfo.Verb = "print";
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Program Files (x86)\Adobe\Reader 11.0\Reader\AcroRd32.exe";
            startInfo.Arguments = "/p /h \"C:\\test.pdf\" \"HPPrinter\"";
            //startInfo.UserName = impersonationUsername;
            //startInfo.Domain = impersonationDomain;
            //SecureString ss = new SecureString();
            //for (int i = 0; i < impersonationPassword.Length; i++)
            //{
            //    ss.AppendChar(impersonationPassword[i]);
            //}
            //startInfo.Password = ss;
            process.StartInfo = startInfo;
            process.Start();

            //string strCmdText;
            //strCmdText = "/p /h \"C:/test.pdf\" \"HPPrinter\"";
            //System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Adobe\Reader 11.0\Reader\AcroRd32.exe", strCmdText);

            return Request.CreateResponse(HttpStatusCode.OK, "Hello Print World");
        }

        [HttpGet]
        public HttpResponseMessage GetPayPalOrders()
        {
            var pps = new PayPalService();

            var orders = pps.GetPayPalOrders();

            return Request.CreateResponse(HttpStatusCode.OK, orders);
        }

        [HttpGet]
        public HttpResponseMessage GetBillingPlan()
        {
            var pps = new PayPalService();

            var plan = pps.GetBillingPlan();

            return Request.CreateResponse(HttpStatusCode.OK, plan);
        }

        [HttpGet]
        public HttpResponseMessage GetAgreement()
        {
            var agreement = new Agreement();
            try 
            { 
                var pps = new PayPalService();
                agreement = pps.GetAgreement();
            }
            catch (PayPal.PaymentsException ex)
            {
                logPayPalException(ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, agreement);
        }

        [HttpGet]
        public HttpResponseMessage PrintFedExLabel()
        {
            var ss = new ShippingService();
            var shipTo = new zCush.Common.Dtos.Address();

            //EXPRESS TEST ADDRESS
            shipTo.ContactName = "Amazon Warehouse";
            shipTo.CompanyName = "Amazon";
            shipTo.AddressLine1 = "800 N 75th Ave";
            shipTo.City = "Phoenix";
            shipTo.State = "AZ";
            shipTo.ZipCode = "85043";

            //GROUND TEST ADDRESS
            //shipTo.ContactName = "FedEx Ground";
            //shipTo.CompanyName = "Barcode Analysis";
            //shipTo.AddressLine1 = "1000 FedEx Drive";
            //shipTo.City = "Moon Township";
            //shipTo.State = "PA";
            //shipTo.ZipCode = "15108";

            ss.CreateFedExLabel(shipTo, Shipping3PartyAccounts.Amazon, "12345");

            return Request.CreateResponse(HttpStatusCode.OK, "Success");
        }

        [HttpGet]
        public HttpResponseMessage CreatePayPalOrder()
        {
            try
            {
                var apiContext = PayPalConfiguration.GetAPIContext();
                CreateBillingAgreement(apiContext);
            }
            catch (PayPal.PaymentsException ex)
            {
                logPayPalException(ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Success");
        }

        [HttpGet]
        public HttpResponseMessage GetTransactionDetails()
        {
            var transactionDetails = new GetTransactionDetailsResponseType();

            try
            {
                var pps = new PayPalService();
                transactionDetails = pps.GetTransactionDetails("abc");
            }
            catch (PayPal.PaymentsException ex)
            {
                logPayPalException(ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, transactionDetails);
        }

        //[HttpGet]
        //public HttpResponseMessage GetTransactions()
        //{
        //    var transactions = new List<PaymentTransactionSearchResultType>();

        //     try
        //     {
        //       var pps = new PayPalService();
        //       transactions = pps.GetTransactions();
        //     }
        //     catch (PayPal.PaymentsException ex)
        //     {
        //         logPayPalException(ex);
        //     }

        //     return Request.CreateResponse(HttpStatusCode.OK, transactions);
        //}

        [HttpGet]
        public HttpResponseMessage GetPayments()
        {
            var paymentHistory = new PaymentHistory();

            try
            {
                var pps = new PayPalService();
                paymentHistory = pps.GetPayments();
            }
            catch (PayPal.PaymentsException ex)
            {
                logPayPalException(ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, paymentHistory);
        }

        [HttpGet]
        public HttpResponseMessage GetPayment()
        {
            var payment = new Payment();

            try
            {
                var pps = new PayPalService();
                payment = pps.GetPayment();
            }
            catch (PayPal.PaymentsException ex)
            {
                logPayPalException(ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, payment);
        }

        [HttpGet]
        public HttpResponseMessage GetPlans()
        {
            var planList = new PlanList();

            try
            {
                var pps = new PayPalService();
                planList = pps.GetPlans();
            }
            catch (PayPal.PaymentsException ex)
            {
                logPayPalException(ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, planList);
        }

        private void CreateBillingAgreement(APIContext apiContext)
        {
            // Before we can setup the billing agreement, we must first create a
            // billing plan that includes a redirect URL back to this test server.
            var plan = CreatePlanObject(HttpContext.Current);
            var guid = Convert.ToString((new Random()).Next(100000));
            plan.merchant_preferences.return_url = "http://localhost/zcush/api/zCushtest/ExecuteBillingAgreement?guid=" + guid;

            var createdPlan = plan.Create(apiContext);

            // Activate the plan
            var patchRequest = new PatchRequest()
            {
                new Patch()
                {
                    op = "replace",
                    path = "/",
                    value = new Plan() { state = "ACTIVE" }
                }
            };

            createdPlan.Update(apiContext, patchRequest);

            // With the plan created and activated, we can now create the billing agreement.

            var payer = new Payer() { payment_method = "paypal" };
            var shippingAddress = new ShippingAddress()
            {
                line1 = "111 First Street",
                city = "Saratoga",
                state = "CA",
                postal_code = "95070",
                country_code = "US"
            };

            var agreement = new Agreement()
            {
                name = "T-Shirt of the Month Club",
                description = "Agreement for T-Shirt of the Month Club",
                payer = payer,
                plan = new Plan() { id = createdPlan.id },
                shipping_address = shippingAddress
            };

            // Create the billing agreement.
            var createdAgreement = agreement.Create(apiContext);

            // Get the redirect URL to allow the user to be redirected to PayPal to accept the agreement.
            var links = createdAgreement.links.GetEnumerator();

            while (links.MoveNext())
            {
                var link = links.Current;
                if (link.rel.ToLower().Trim().Equals("approval_url"))
                {
                    //this.flow.RecordRedirectUrl("Redirect to PayPal to approve billing agreement...", link.href);
                    HttpContext.Current.Response.Redirect(link.href);
                }
            }
        }

        [HttpGet]
        public HttpResponseMessage ExecuteBillingAgreement([FromUri]string token)
        {
            var agreement = new Agreement() { token = token };
            var executedAgreement = agreement.Execute(PayPalConfiguration.GetAPIContext());
            return Request.CreateResponse(HttpStatusCode.OK, executedAgreement);
        }

        private Currency GetCurrency(string value)
        {
            return new Currency() { value = value, currency = "USD" };
        }

        public Plan CreatePlanObject(HttpContext httpContext)
        {
            // ### Create the Billing Plan
            // Both the trial and standard plans will use the same shipping
            // charge for this example, so for simplicity we'll create a
            // single object to use with both payment definitions.
            var shippingChargeModel = new ChargeModel()
            {
                type = "SHIPPING",
                amount = GetCurrency("9.99")
            };

            // Define the plan and attach the payment definitions and merchant preferences.
            // More Information: https://developer.paypal.com/webapps/developer/docs/api/#create-a-plan
            return new Plan
            {
                name = "T-Shirt of the Month Club Plan",
                description = "Monthly plan for getting the t-shirt of the month.",
                type = "fixed",
                // Define the merchant preferences.
                // More Information: https://developer.paypal.com/webapps/developer/docs/api/#merchantpreferences-object
                merchant_preferences = new MerchantPreferences()
                {
                    setup_fee = GetCurrency("1"),
                    return_url = httpContext.Request.Url.ToString(),
                    cancel_url = httpContext.Request.Url.ToString() + "?cancel",
                    auto_bill_amount = "YES",
                    initial_fail_amount_action = "CONTINUE",
                    max_fail_attempts = "0"
                },
                payment_definitions = new List<PaymentDefinition>
                {
                    // Define a trial plan that will only charge $9.99 for the first
                    // month. After that, the standard plan will take over for the
                    // remaining 11 months of the year.
                    new PaymentDefinition()
                    {
                        name = "Trial Plan",
                        type = "TRIAL",
                        frequency = "MONTH",
                        frequency_interval = "1",
                        amount = GetCurrency("9.99"),
                        cycles = "1",
                        charge_models = new List<ChargeModel>
                        {
                            new ChargeModel()
                            {
                                type = "TAX",
                                amount = GetCurrency("1.65")
                            },
                            shippingChargeModel
                        }
                    },
                    // Define the standard payment plan. It will represent a monthly
                    // plan for $19.99 USD that charges once month for 11 months.
                    new PaymentDefinition
                    {
                        name = "Standard Plan",
                        type = "REGULAR",
                        frequency = "MONTH",
                        frequency_interval = "1",
                        amount = GetCurrency("19.99"),
                        // > NOTE: For `IFNINITE` type plans, `cycles` should be 0 for a `REGULAR` `PaymentDefinition` object.
                        cycles = "11",
                        charge_models = new List<ChargeModel>
                        {
                            new ChargeModel
                            {
                                type = "TAX",
                                amount = GetCurrency("2.47")
                            },
                            shippingChargeModel
                        }
                    }
                }
            };
        }

        private void logPayPalException(PayPal.PaymentsException ex)
        {

            // Get the details of this exception with ex.Details.  If you have logging setup for your project, this information will also be automatically logged to your logfile.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Error:    " + ex.Details.name);
            sb.AppendLine("Message:  " + ex.Details.message);
            sb.AppendLine("URI:      " + ex.Details.information_link);
            sb.AppendLine("Debug ID: " + ex.Details.debug_id);

            if (ex.Details.details != null)
            {
                foreach (var errorDetails in ex.Details.details)
                {
                    sb.AppendLine("Details:  " + errorDetails.field + " -> " + errorDetails.issue);
                }
            }

            Debug.Write(sb.ToString());
        }
    }
}
