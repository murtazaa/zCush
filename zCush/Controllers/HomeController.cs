using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using zCush.Orders;
using zCush.Partners.Amazon;
using zCush.Partners.WebSite;

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
            var wsOrder = new WebSiteOrder();

            var wsPos = wsOrder.GetWebSiteOrders();

            return View("~/Views/Home/PurchaseOrders.cshtml", wsPos);
        }
    }
}
