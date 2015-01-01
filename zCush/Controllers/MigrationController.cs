using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using zCush.Data;
using zCush.Partners.Amazon;

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
    }
}