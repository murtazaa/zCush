//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace zCush.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class ThirdPartyShipmentCarrierAccount
    {
        public int ID { get; set; }
        public int CustomerId { get; set; }
        public int CarrierTypeId { get; set; }
        public string AccountNumber { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }
}