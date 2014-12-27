using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zCush.Common.Dtos;

namespace zCush.Partners
{
    public class PurchaseOrder
    {
        public PurchaseOrder()
        {
            POLineItems = new List<POLineItem>();
        }

        public string PONumber { get; set; }
        public string CustomerId { get; set; }
        public DateTime PODate { get; set; }
        public Address ShipAddress { get; set; }
        public List<POLineItem> POLineItems { get; set; }
        public decimal Total
        {
            get
            {
                return POLineItems.Sum(pli => pli.LineItemTotal);
            }
        }
    }

    public class POLineItem
    {
        public string POLineItemId { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public decimal LineItemTotal {
            get
            {
                return Price * Quantity;
            }
        }
    }
}