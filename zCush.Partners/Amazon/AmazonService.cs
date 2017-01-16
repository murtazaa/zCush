using ActiveUp.Net.Mail;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using zCush.Frameworks.AddressParser;

namespace zCush.Partners.Amazon
{

    public class AmazonLabelAddresses
    {
        public CarrierType Carrier { get; set; }
        public zCush.Common.Dtos.Address Address { get; set; }
    }

    public class AmazonService
    {

        public List<PurchaseOrder> GetAmazonPOs(){
            var amazonOrders = new List<PurchaseOrder>();           
            
            amazonOrders.Add(GetAmazonPurchaseOrder(@"C:\zCush\Amazon\PO\T1.xml"));
            amazonOrders.Add(GetAmazonPurchaseOrder(@"C:\zCush\Amazon\PO\T2.xml"));
            amazonOrders.Add(GetAmazonPurchaseOrder(@"C:\zCush\Amazon\PO\T3.xml"));

            return amazonOrders;
        }

        private PurchaseOrder GetAmazonPurchaseOrder(string fileName)
        {
            var products = Products.GetAllProducts().ToDictionary(k => k.SKU, v => v.Description);
            var amazonWarehouses = AmazonWarehouses.GetAllAmazonWarehouseAddresses();  

            XElement xelement = XElement.Load(fileName);
            var polineItem = from pli in xelement.XPathSelectElements("./FunctionGroup/Transaction/Loop")
                             where (string)pli.Attribute("Name") == "Baseline Item Data"
                             select pli;

            var amazonWarehouseInfo = from awh in xelement.XPathSelectElements("./FunctionGroup/Transaction/Loop")
                                      where (string)awh.Attribute("LoopId") == "N1"
                                      select awh;

            var amazonWarehouseSanCode = amazonWarehouseInfo.First().XPathSelectElement("./N1/N104").Value.ToString();
            var apo = new PurchaseOrder
            {
                CustomerId = "Amazon",
                PODate = DateTime.ParseExact(xelement.XPathSelectElement("./FunctionGroup/Transaction/BEG/BEG05").Value.ToString(),
                                    "yyyyMMdd",
                                    System.Globalization.CultureInfo.InvariantCulture),
                PONumber = xelement.XPathSelectElement("./FunctionGroup/Transaction/BEG/BEG03").Value.ToString(),
                ShipAddress = amazonWarehouses.FirstOrDefault(aw => aw.SanCode == amazonWarehouseSanCode)
            };
            
            foreach (XElement lineItem in polineItem)
            {
                var sku = lineItem.XPathSelectElement("./PO1/PO107").Value.ToString();
                if (products.ContainsKey(sku))
                {
                    apo.POLineItems.Add(new POLineItem
                    {
                        SKU = sku,
                        Quantity = int.Parse(lineItem.XPathSelectElement("./PO1/PO102").Value.ToString()),
                        Price = Decimal.Parse(lineItem.XPathSelectElement("./PO1/PO104").Value.ToString()),
                        ProductDescription = products[sku]
                    });
                }
            }

            return apo;
        }

        public List<AmazonLabelAddresses> GetAmazonAddresses(DateTime date)
        {
            var ImapServer = "imap.secureserver.net";
            var userName = "murtaza@zcush.com";
            var password = "godri110";
            var amazonLabelAddresses = new List<AmazonLabelAddresses>();
            var amazomWarehouses = AmazonWarehouses.GetAllAmazonWarehouseAddresses();
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
                if (msgCount > 0)
                {
                    var newMessage = inbox.Fetch.MessageObject(msgCount--);
                    while (newMessage.Date >= date)
                    {
                        var isAmazonLabelEmail = IsAmazonLabelEmail(newMessage.Subject);

                        if (isAmazonLabelEmail)
                        {
                            var carrier = GetCarrier(newMessage.Subject);

                            var amazonLabelAddress = new AmazonLabelAddresses
                            {
                                Carrier = carrier,
                                Address = ExtractAddressFromEmail(newMessage.BodyText.Text, amazomWarehouses)
                            };
                            amazonLabelAddresses.Add(amazonLabelAddress);
                        }

                        newMessage = inbox.Fetch.MessageObject(msgCount--);
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

            amazonLabelAddresses = amazonLabelAddresses.OrderBy(ala => ala.Carrier).ToList();
            return amazonLabelAddresses;
        }

        private bool IsAmazonLabelEmail(string emailSubject)
        {
            var culture = new CultureInfo("EN-US");

            return culture.CompareInfo.IndexOf(emailSubject, "Routing Request response from Amazon", CompareOptions.IgnoreCase) >= 0 &&
                   culture.CompareInfo.IndexOf(emailSubject, "RE:", CompareOptions.IgnoreCase) == -1 &&
                   culture.CompareInfo.IndexOf(emailSubject, "FW:", CompareOptions.IgnoreCase) == -1;
        }

        private zCush.Common.Dtos.Address ExtractAddressFromEmail(string amazonEmailBody, List<AmazonWarehouse> amazonWarehouses)
        {
            var address = new zCush.Common.Dtos.Address();
            var culture = new CultureInfo("EN-US");
            var startString = "DELIVERY";
            var endString = "RECEIVING DEPARTMENT";
            var indexOfAddressStart = culture.CompareInfo.IndexOf(amazonEmailBody, startString, CompareOptions.IgnoreCase);
            var indexOfAddressEnd = culture.CompareInfo.IndexOf(amazonEmailBody, endString, CompareOptions.IgnoreCase);
            var addressInBetween = amazonEmailBody.Substring(indexOfAddressStart + 8, indexOfAddressEnd - indexOfAddressStart - 8);

            var identifier = addressInBetween.Substring(addressInBetween.Length - 6).Trim();

            address.CompanyName = identifier;

            if(identifier.Length == 4)
            {
                var aw = amazonWarehouses.FirstOrDefault(awh => awh.Identifier == identifier);
                if(aw != null)
                {
                    return aw;
                }
            }

            //Now need to use the address parser could not resolve using the identifier
            var indexOfFirstLineBreak = addressInBetween.IndexOf(Environment.NewLine);
            var rawAddressString = addressInBetween.Substring(indexOfFirstLineBreak);

            var ap = new AddressParser();
            var parsedAddress = ap.ParseAddress(rawAddressString);

            address.ContactName = rawAddressString.Substring(0, culture.CompareInfo.IndexOf(rawAddressString, parsedAddress.Number, CompareOptions.IgnoreCase));
            address.AddressLine1 = parsedAddress.StreetLine;
            address.AddressLine2 = parsedAddress.SecondaryUnit + " " + parsedAddress.SecondaryNumber;
            address.City = parsedAddress.City;
            address.State = parsedAddress.State;
            address.ZipCode = parsedAddress.Zip;

            address.RawAddress = rawAddressString;
            return address;
        }

        private CarrierType GetCarrier(string emailSubject)
        {
            var culture = new CultureInfo("EN-US");

            if (culture.CompareInfo.IndexOf(emailSubject, "FDE", CompareOptions.IgnoreCase) >= 0)
            {
                return CarrierType.FedEx;
            }

            if (culture.CompareInfo.IndexOf(emailSubject, "UPSN", CompareOptions.IgnoreCase) >= 0)
            {
                return CarrierType.UPS;
            }

            return CarrierType.UnKnown;
        }

    }
}