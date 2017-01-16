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
            //get { return "T0G3yIomOQbbrWrqS7XYiQ"; } //Test Key
            get { return "x018Vgs5Jf0b4ouAXXcZCA"; } //Prod Key
        }       

        public void CreateFedExLabel(zCush.Common.Dtos.Address shipToAddress, Shipping3PartyAccounts thirdPartyAccount, string referenceNumber, int qty)
        {
            var shipment = CreateFedExShipment(shipToAddress, referenceNumber, thirdPartyAccount, qty);
            if (!shipment.messages.Any())
            {
                var fedExGroudShipmentRate = shipment.rates.First(r => r.carrier == "FedEx" && r.service == "FEDEX_GROUND");
                BuyAndPrintShipment(shipment, fedExGroudShipmentRate);
            }
            else
            {
                //Unable to create a label for this one. 
            }

            //var fedExExpressShipment = shipment.rates.First(r => r.carrier == "FedEx" && r.service == "FEDEX_EXPRESS_SAVER");
        }

        public void CreateUPSGroundLabel(zCush.Common.Dtos.Address shipToAddress, Shipping3PartyAccounts accountType, string referenceNumber, int qty)
        {
            var shipment = CreateUPSShipment(shipToAddress, referenceNumber, accountType, qty);
            var upsGroundShipmentRate = shipment.rates.First(r => r.carrier == "UPS" && r.service == "Ground");
            BuyAndPrintShipment(shipment, upsGroundShipmentRate);            
        }

        private Shipment CreateUPSShipment(zCush.Common.Dtos.Address shipToAddress, string referenceNumber, Shipping3PartyAccounts accontyType, int qty)
        {
            Client.apiKey = ClientKey;

            Dictionary<string, object> fromAddress = new Dictionary<string, object>() {
                {"company", "zCush"},
                {"street1", "661 Abbington Drive"},
                {"street2", "G14"},
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
                {"zip", shipToAddress.ZipCode}
            };

            var thirdPartyAccountInfo = GetUPSAccountInfo(accontyType);

            var shipment = Shipment.Create(new Dictionary<string, object>() {
                {"parcel", new Dictionary<string, object>() {
                  {"length", 18},
                  {"width", 12},
                  {"height", 8},
                  {"weight", qty * 48}}
                },
                {"to_address", toAddress},
                {"from_address", fromAddress}, 
                {"options" , new Dictionary<string, object>(){
                    {"bill_third_party_account",  thirdPartyAccountInfo.AccountNumber},
                    {"bill_third_party_country", thirdPartyAccountInfo.Country},
                    {"bill_third_party_postal_code", thirdPartyAccountInfo.ZipCode}, 
                    {"print_custom_1", referenceNumber}}
                },
                {"reference", referenceNumber}
            });

            return shipment;
        }

        private Shipment CreateFedExShipment(zCush.Common.Dtos.Address shipToAddress, string referenceNumber, Shipping3PartyAccounts accontyType, int qty)
        {
            Client.apiKey = ClientKey;

            Dictionary<string, object> fromAddress = new Dictionary<string, object>() {
                {"company", "zCush"},
                {"street1", "661 Abbington Drive"},
                {"street2", "G14"},
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

            var newShipment = new Dictionary<string, object>() {
                    {"parcel", new Dictionary<string, object>() {
                      {"length", 18},
                      {"width", 12},
                      {"height", 8},
                      {"weight", qty * 48}}
                    },
                    {"to_address", toAddress},
                    {"from_address", fromAddress}
            };

            if (accontyType != Shipping3PartyAccounts.None)
            {
                var thirdPartyAccountInfo = GetFedExAccountInfo(accontyType);

                newShipment.Add("options" , new Dictionary<string, object>(){
                        {"bill_third_party_account",  thirdPartyAccountInfo.AccountNumber}, 
                        {"print_custom_1", referenceNumber}});
                newShipment.Add("reference", referenceNumber);
            }

            var shipment = Shipment.Create(newShipment);
            return shipment;
        }

        private void BuyAndPrintShipment(Shipment shipment, Rate rate)
        {
            shipment.Buy(rate.id);

            var ps = new PrintingService();
            ps.DownloadFileAndPrint(shipment.postage_label.label_url, "png");
        }     
        
        private Shipping3PartyAccountInfo GetUPSAccountInfo(Shipping3PartyAccounts accountType)
        {
            switch(accountType)
            {
                case Shipping3PartyAccounts.WayFair:
                    return new Shipping3PartyAccountInfo{
                        AccountNumber = "Y36935",
                        Country = "US",
                        ZipCode = "02116"
                    };
                case Shipping3PartyAccounts.Amazon:
                    return new Shipping3PartyAccountInfo
                    {
                        AccountNumber = "1Y750E",
                        Country = "US",
                        ZipCode = "98109"
                    };
            }

            return null;
        }

        private Shipping3PartyAccountInfo GetFedExAccountInfo(Shipping3PartyAccounts accountType)
        {
            switch (accountType)
            {
                case Shipping3PartyAccounts.Amazon:
                    return new Shipping3PartyAccountInfo
                    {
                        AccountNumber = "471163562"
                    };
            }

            return null;
        }

    }

    public enum Shipping3PartyAccounts {
        None,
        WayFair,
        Amazon,
        UnbeatableSales
    }

    public class Shipping3PartyAccountInfo{
        public string AccountNumber { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
    }
}