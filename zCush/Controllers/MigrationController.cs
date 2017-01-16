using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using zCush.Common.Enums;
using zCush.Data;
using zCush.Partners;
using zCush.Partners.Amazon;
using zCush.Partners.WayFair;

namespace zCush.Controllers
{
    public class MigrationController : Controller
    {
        [HttpGet]
        public JsonResult MigrateAmazonWarehouses()
        {
            var aws = AmazonWarehouses.GetAllAmazonWarehouseAddresses();
            var zuow = new zCushUnitOfWork();

            foreach (var aw in aws)
            {
                var newAws = new Ref_AmazonWarehouse
                {
                    Identifier = aw.Identifier,
                    SanCode = aw.SanCode,
                    CartonQtySanCode = aw.CartonQtySanCode,
                    Name = aw.ContactName,
                    Address = new Address
                    {
                        AddressLine1 = aw.AddressLine1,
                        AddressLine2 = aw.AddressLine2,
                        City = aw.City,
                        State = aw.State,
                        ZipCode = aw.ZipCode, 
                        Country = "US"
                    }
                };

                zuow.Ref_AmazonWarehouse.Add(newAws);
            }
            zuow.SaveChangesAsync();

            return Json("Amazon Warehouses Migrated Successfully", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MigrateWayFairOrders()
        {
            

            return Json("WayFair Orders Migrated Successfully", JsonRequestBehavior.AllowGet);
        }


        private void InsertPos(List<PurchaseOrder> pos)
        {
            var zuow = new zCushUnitOfWork();
            var customerId = zuow.Customers.FirstOrDefault(ct => ct.Name == BusinessCustomerName.WayFair.ToString()).ID;
            var wfOrder = new WayfairOrder();

            var wfPos = wfOrder.GetWayFairOrders();

            foreach (var po in pos)
            {
                var newOrder = new Order
                {
                    Address = new Address
                    {
                        AddressLine1 = po.ShipAddress.AddressLine1,
                        AddressLine2 = po.ShipAddress.AddressLine2,
                        City = po.ShipAddress.City,
                        State = po.ShipAddress.State,
                        ZipCode = po.ShipAddress.ZipCode,
                        Country = "US"
                    },
                    CustomerId = customerId,
                    OrderStatusId = (int)OrderStatusType.Paid,
                    PONumber = po.PONumber,
                    PODate = po.PODate
                };                

                foreach (var poli in po.POLineItems)
                {
                    var productId = zuow.Products.FirstOrDefault(pd => pd.SKU == poli.SKU).ID;

                    newOrder.OrderLineItems.Add(new OrderLineItem
                    {
                        ProductId = productId,
                        Quantity = poli.Quantity,
                        Price = poli.Price
                    });
                }

                zuow.Orders.Add(newOrder);
            }

            zuow.SaveChangesAsync();
        }
    }
}