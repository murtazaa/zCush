using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zCush.Partners
{
    public static class Products
    {
        public static List<Product> GetAllProducts()
        {
            var products = new List<Product>();

            products.Add(new Product
            {
                SKU = "BNM-CT-GR-EM",
                Description = "Googly Green Nap Mat"
            });

            products.Add(new Product
            {
                SKU = "BNM-CT-PN-EM",
                Description = "Cotton Candy Nap Mat"
            });

            products.Add(new Product
            {
                SKU = "BNM-CT-YL-EM",
                Description = "Dainty Duckling Nap Mat"
            });


            products.Add(new Product
            {
                SKU = "BNM-CT-BL-EM",
                Description = "Plushy Paws Nap Mat"
            });

            products.Add(new Product
            {
                SKU = "BNM-CT-YL-OW",
                Description = "Happy Hoot Nap Mat"
            });

            products.Add(new Product
            {
                SKU = "BNM-CT-BL-MK",
                Description = "Precious Peekaboo Nap Mat"
            });

            products.Add(new Product
            {
                SKU = "BNM-MC-BL-RF",
                Description = "Silky Sky Nap Mat"
            });

            products.Add(new Product
            {
                SKU = "BNM-MC-HP-RF",
                Description = "Berry Beginning Nap Mat"
            });

            products.Add(new Product
            {
                SKU = "BNM-MC-GY-RF",
                Description = "Soothing Slumber Nap Mat"
            });

            products.Add(new Product
            {
                SKU = "BNM-MC-GR-RF",
                Description = "Firry Forest Nap Mat"
            });

            products.Add(new Product
            {
                SKU = "BNM-MC-WH-RF",
                Description = "Whimsical White Nap Mat"
            });

            products.Add(new Product
            {
                SKU = "BNMC-CT-YL-OW",
                Description = "Happy Hoot Nap Mat Cover"
            });

            return products;
        }
    }

    public class Product
    {
        public string SKU { get; set; }
        public string Description { get; set; }
    }
}