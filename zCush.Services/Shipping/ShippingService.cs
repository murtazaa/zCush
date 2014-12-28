using System;
using System.Collections.Generic;
using System.Linq;
using EasyPost;
using System.Drawing;
using System.Drawing.Printing;
using System.Net;
using System.Security.Principal;
using zCush.Services.Printing;

namespace zCush.Services.Shipping
{
    public class ShippingService
    {

        private string ClientKey
        {
            get { return "T0G3yIomOQbbrWrqS7XYiQ"; }
            //Test key "T0G3yIomOQbbrWrqS7XYiQ"
            //Prod key "x018Vgs5Jf0b4ouAXXcZCA"
        }       

        public void CreateFedExLabel(zCush.Common.Dtos.Address shipToAddress)
        {
            var shipment = CreateShipment(shipToAddress, true);
            var fedExGroudShipmentRate = shipment.rates.First(r => r.carrier == "FedEx" && r.service == "FEDEX_GROUND");
            BuyAndPrintShipment(shipment, fedExGroudShipmentRate);
            //var fedExExpressShipment = shipment.rates.First(r => r.carrier == "FedEx" && r.service == "FEDEX_EXPRESS_SAVER");
        }

        public void CreateUPSGroundLabel(zCush.Common.Dtos.Address shipToAddress)
        {
            var shipment = CreateShipment(shipToAddress);
            var upsGroundShipmentRate = shipment.rates.First(r => r.carrier == "UPS" && r.service == "Ground");
            BuyAndPrintShipment(shipment, upsGroundShipmentRate);            
        }

        private Shipment CreateShipment(zCush.Common.Dtos.Address shipToAddress, bool requiresPhoneNumber = false)
        {
            Client.apiKey = ClientKey;

            Dictionary<string, object> fromAddress = new Dictionary<string, object>() {
                {"company", "zCush"},
                {"street1", "936 Jamestown Road"},
                {"street2", ""},
                {"city", "East Windsor"},
                {"state", "NJ"},
                {"phone", "7324474916"},
                {"zip", "08520"}
            };

            Dictionary<string, object> toAddress = new Dictionary<string, object>() {
                {"name", shipToAddress.ContactName},
                {"company", shipToAddress.CompanyName},
                {"street1", shipToAddress.AddressLine1},
                {"street2", shipToAddress.AddressLine2},
                {"city", shipToAddress.City},
                {"state", shipToAddress.State},
                {"zip", shipToAddress.ZipCode},
                {"phone", "7324474916"}
            };

            var shipment = Shipment.Create(new Dictionary<string, object>() {
                {"parcel", new Dictionary<string, object>() {
                  {"length", 18},
                  {"width", 12},
                  {"height", 8},
                  {"weight", 3}}
                },
                {"to_address", toAddress},
                {"from_address", fromAddress}
            });

            return shipment;
        }

        private void BuyAndPrintShipment(Shipment shipment, Rate rate)
        {
            shipment.Buy(rate.id);

            var ps = new PrintingService();
            ps.DownloadFileAndPrint(shipment.postage_label.label_url);
        }

        
    }
}