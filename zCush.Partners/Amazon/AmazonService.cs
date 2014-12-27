using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;

namespace zCush.Partners.Amazon
{
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

    }
}