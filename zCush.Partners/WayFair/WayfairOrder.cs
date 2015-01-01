using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using zCush.Frameworks.AddressParser;
using zCush.Services.Email;
using zCush.Services.Printing;
using zCush.Services.Shipping;

namespace zCush.Partners.WayFair
{
    public class WayfairOrder
    {
        public List<PurchaseOrder> GetWayFairOrders()
        {
            var wayFairPos = new List<PurchaseOrder>();
            var es = new EmailService();
            var emails = es.GetAllEmailsOn(DateTime.Today.AddDays(-2).AddHours(6));

            foreach (var email in emails)
            {
                var isWayFairEmail = IsWayFairOrderEmail(email.Subject);

                if (isWayFairEmail)
                {
                    var ShipToAddress = ExtractAddressFromEmail(email.BodyText.Text);
                    var PoNumber = ExtractPOFromEmailSubject(email.Subject);
                    var po = new PurchaseOrder
                    {
                        PONumber = PoNumber,
                        ShipAddress = ShipToAddress
                    };

                    PrintPackingSlip(email.BodyText.Text);
                    var fds = new ShippingService();
                    fds.CreateUPSGroundLabel(ShipToAddress, Shipping3PartyAccounts.WayFair, PoNumber);

                    wayFairPos.Add(po);
                }
            }

            return wayFairPos;
        }

        private void PrintPackingSlip(string wayfairEmailBody)
        {
            var culture = new CultureInfo("EN-US");
            var urlStartString = "Packing Slip URL:";
            var indexOfPackingSlipURLStart = culture.CompareInfo.IndexOf(wayfairEmailBody, urlStartString, CompareOptions.IgnoreCase);

            var packingSlipUrl = wayfairEmailBody.Substring(indexOfPackingSlipURLStart + urlStartString.Length);

            var ps = new PrintingService();
            ps.DownloadFileAndPrint(packingSlipUrl, "pdf");

        }

        private string ExtractPOFromEmailSubject(string emailSubject)
        {
            var culture = new CultureInfo("EN-US");
            var startString = "CS";
            var endString = " - (";

            var indexOfStart = culture.CompareInfo.IndexOf(emailSubject, startString, CompareOptions.IgnoreCase);
            var indexOfEnd = culture.CompareInfo.IndexOf(emailSubject, endString, CompareOptions.IgnoreCase);

            return emailSubject.Substring(indexOfStart, indexOfEnd - indexOfStart);
        }

        private zCush.Common.Dtos.Address ExtractAddressFromEmail(string wayfairEmailBody)
        {
            var address = new zCush.Common.Dtos.Address();
            var culture = new CultureInfo("EN-US");

            var indexOfAddressStart = culture.CompareInfo.IndexOf(wayfairEmailBody, "ship to:", CompareOptions.IgnoreCase);
            var indexOfAddressEnd = culture.CompareInfo.IndexOf(wayfairEmailBody, ", US", CompareOptions.IgnoreCase);
            var addressInBetween = wayfairEmailBody.Substring(indexOfAddressStart + 8, indexOfAddressEnd - indexOfAddressStart - 8);
            var rawAddressString = addressInBetween.Replace("=", "");
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

        private bool IsWayFairOrderEmail(string emailSubject)
        {
            var culture = new CultureInfo("EN-US");

            return culture.CompareInfo.IndexOf(emailSubject, "PO# CS", CompareOptions.IgnoreCase) >= 0 &&
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