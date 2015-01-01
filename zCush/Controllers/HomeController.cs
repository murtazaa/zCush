using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using zCush.Data;
using zCush.Orders;
using zCush.Partners.Amazon;
using zCush.Partners.PayPal;
using zCush.Partners.WayFair;
using zCush.Partners.WebSite;
using zCush.Services.Email;

namespace zCush.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpGet]
        public ViewResult PurchaseOrders()
        {
            var amazonService = new AmazonService();

            var purchaseOrders = amazonService.GetAmazonPOs();

            return View("~/Views/Home/PurchaseOrders.cshtml", purchaseOrders);
        }

        [HttpGet]
        public ViewResult WebSiteOrders()
        {
            var wsOrder = new PayPalService();

            var wsPos = wsOrder.GetPayPalOrders();

            return View("~/Views/Home/PurchaseOrders.cshtml", wsPos);
        }

        [HttpGet]
        public ViewResult WayfairOrders()
        {
            var wfOrder = new WayfairOrder();

            var wfPos = wfOrder.GetWayFairOrders();

            return View("~/Views/Home/PurchaseOrders.cshtml", wfPos);
        }

        [HttpGet]
        public ViewResult GetProducts()
        {
            var zuow = new zCushUnitOfWork();

            var zProducts = zuow.Products.ToList();

            return View("~/Views/Home/Products.cshtml", zProducts);
        }

        [HttpGet]
        public ViewResult GetAmazonShippingLabels()
        {
            var ams = new AmazonService();

            var amazonOrders = ams.GetAmazonAddresses(DateTime.Today.AddDays(-120));

            return View("~/Views/Home/AmazonLabelAddresses.cshtml", amazonOrders);
        }


        [HttpGet]
        public ViewResult GetzCushEmails()
        {
            var es = new EmailService();

            var emails = es.GetAllEmailsOn(DateTime.Today.AddDays(-1));

            return View("~/Views/Home/zCushEmails.cshtml", emails);
        }        
    }
}
