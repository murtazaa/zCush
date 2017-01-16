using RazorEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.WinForms;
using zCush.Data;
using zCush.Orders;
using zCush.Partners;
using zCush.Partners.Amazon;
using zCush.Partners.PayPal;
using zCush.Partners.WayFair;
using zCush.Partners.WebSite;
using zCush.Services.Email;
using zCush.Services.Shipping;

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

            var amazonOrders = ams.GetAmazonAddresses(DateTime.Today.AddDays(-3));
            var ss = new ShippingService();

            foreach (var ao in amazonOrders)
            {
                if (ao.Carrier == CarrierType.FedEx)
                {
                    ss.CreateFedExLabel(ao.Address, Shipping3PartyAccounts.Amazon, "zCush Order", 1);
                }

                if(ao.Carrier == CarrierType.UPS)
                {
                    ss.CreateUPSGroundLabel(ao.Address, Shipping3PartyAccounts.Amazon, "zCush Orer", 1);
                }
            }

            return View("~/Views/Home/AmazonLabelAddresses.cshtml", amazonOrders);
        }


        [HttpGet]
        public ViewResult GetzCushEmails()
        {
            var es = new EmailService();

            var emails = es.GetAllEmailsOn(DateTime.Today.AddDays(-1));

            return View("~/Views/Home/zCushEmails.cshtml", emails);
        }

        //private delegate void ImageLoadHanderEvent(object sender, HtmlImageLoadEventArgs e);
        private event EventHandler<HtmlImageLoadEventArgs> ImageLoadEvent;

        [HttpGet]
        public ActionResult PrintPackingList()
        {
            var wfOrder = new WayfairOrder();

            var wfPos = wfOrder.GetWayFairOrders();
            var viewString = RenderRazorViewToString("~/Views/Home/PackingList.cshtml", wfPos[0]);
            ImageLoadEvent += HomeController_ImageLoadEvent;
            //var htmlImageLoadEventHandler = new EventHandler<HtmlImageLoadEventArgs>(ImageLoadHanderEvent);
            var image = HtmlRender.RenderToImage(html: viewString, size: new Size(210, 297), imageLoad: ImageLoadEvent);

            image.Save(@"C:\shipments\packinglist.png", ImageFormat.Png);
                    
            return Content(viewString);
        }

        void HomeController_ImageLoadEvent(object sender, HtmlImageLoadEventArgs e)
        {
            e.Handled = true;
            e.Attributes.Add("src", @"C:\Users\Murtaza\Documents\visual studio 2013\Projects\zCush\zCush\Assets\Images\zCush_Logo.gif");
            //e.Src = @"C:\Users\Murtaza\Documents\visual studio 2013\Projects\zCush\zCush\Assets\Images\zCush_Logo.gif";
        } 

        //private void ImageLoadHandlerEvent(object source, HtmlImageLoadEventArgs e){

        //}

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
