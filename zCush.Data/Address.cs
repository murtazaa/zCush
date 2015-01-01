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
    
    public partial class Address
    {
        public Address()
        {
            this.BusinessCustomers = new HashSet<BusinessCustomer>();
            this.Orders = new HashSet<Order>();
            this.Ref_AmazonWarehouse = new HashSet<Ref_AmazonWarehouse>();
        }
    
        public int ID { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    
        public virtual ICollection<BusinessCustomer> BusinessCustomers { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Ref_AmazonWarehouse> Ref_AmazonWarehouse { get; set; }
    }
}
