using ActiveUp.Net.Mail;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using zCush.Frameworks.AddressParser;
using zCush.Services.Shipping;

namespace zCush.Partners.WebSite
{
    public class WebSiteOrder
    {

        public List<PurchaseOrder> GetWebSiteOrders()
        {
            var ImapServer = "imap.secureserver.net";
            var userName = "orders@zcush.com";
            var password = "godri786";
            var websitePOs = new List<PurchaseOrder>();

            // We create Imap client
            var imap = new Imap4Client();

            try
            {
                // We connect to the imap4 server
                imap.Connect(ImapServer);

                // Login to mail box
                imap.Login(userName, password);

                Mailbox inbox = imap.SelectMailbox("inbox");
                
                var msgCount = inbox.MessageCount;

                for (int n = msgCount; n > (msgCount - 3); n--)
                {
                    var newMessage = inbox.Fetch.MessageObject(n);

                    var isPayPalEmail = IsPayPalOrderEmail(newMessage.Subject);

                    if (isPayPalEmail)
                    {
                        var po = new PurchaseOrder
                        {
                            PONumber = newMessage.Subject,
                            ShipAddress = ExtractAddressFromEmail(newMessage.BodyText.Text)
                        };

                        websitePOs.Add(po);
                    }
                }                
            }

            catch (Imap4Exception iex)
            {
                //this.AddLogEntry(string.Format("Imap4 Error: {0}", iex.Message));
            }

            catch (Exception ex)
            {
                //this.AddLogEntry(string.Format("Failed: {0}", ex.Message));
            }

            finally
            {
                if (imap.IsConnected)
                {
                    imap.Disconnect();
                }
            }

            return websitePOs;
        }

        private zCush.Common.Dtos.Address ExtractAddressFromEmail(string PayPalEmailBody)
        {
            var address = new zCush.Common.Dtos.Address();
            var culture = new CultureInfo("EN-US");

            var indexOfAddressStart = culture.CompareInfo.IndexOf(PayPalEmailBody, "shipping address", CompareOptions.IgnoreCase);
            if(indexOfAddressStart == -1)
            {
                indexOfAddressStart = culture.CompareInfo.IndexOf(PayPalEmailBody, "ship-to address", CompareOptions.IgnoreCase);
            }
            var indexOfAddressEnd = culture.CompareInfo.IndexOf(PayPalEmailBody, "united states", CompareOptions.IgnoreCase);
            var addressInBetween = PayPalEmailBody.Substring(indexOfAddressStart + 16, indexOfAddressEnd - indexOfAddressStart - 16);
            var rawAddressString = ReplaceString(addressInBetween, "- confirmed", "", StringComparison.OrdinalIgnoreCase);
            rawAddressString = ReplaceString(rawAddressString, "- unconfirmed", "", StringComparison.OrdinalIgnoreCase);
            rawAddressString = rawAddressString.Replace("=", "");
           // rawAddressString = rawAddressString.Replace(",", "");

            var ap = new AddressParser();
            var parsedAddress = ap.ParseAddress(rawAddressString);

            if (!IsPOorAPOAddress(parsedAddress.StreetLine))
            {
                address.ContactName = rawAddressString.Substring(0, culture.CompareInfo.IndexOf(rawAddressString, parsedAddress.Number, CompareOptions.IgnoreCase));
                address.AddressLine1 = parsedAddress.StreetLine;
                address.AddressLine2 = parsedAddress.SecondaryUnit + " " + parsedAddress.SecondaryNumber;
                address.City = parsedAddress.City;
                address.State = parsedAddress.State;
                address.ZipCode = parsedAddress.Zip;

                var fds = new ShippingService();
                //fds.CreateFedExLabel(address);

            }
            address.RawAddress = rawAddressString;
            return address;

        }

        private string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
        {
            var sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        private bool IsPayPalOrderEmail(string emailSubject)
        {
            var culture = new CultureInfo("EN-US");

            return culture.CompareInfo.IndexOf(emailSubject, "payment received", CompareOptions.IgnoreCase) >= 0 &&
                   culture.CompareInfo.IndexOf(emailSubject, "RE:", CompareOptions.IgnoreCase) == -1 &&
                   culture.CompareInfo.IndexOf(emailSubject, "FW:", CompareOptions.IgnoreCase) == -1;
        }

        private bool IsPOorAPOAddress(string addressLine)
        {
            var culture = new CultureInfo("EN-US");

            return addressLine.StartsWith("PO Box", true, culture) ||
                   addressLine.StartsWith("APO", true, culture);
        }

    }
}