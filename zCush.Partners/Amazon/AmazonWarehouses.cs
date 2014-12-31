using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zCush.Common.Dtos;

namespace zCush.Partners.Amazon
{
    public static class AmazonWarehouses
    {
        public static List<AmazonWarehouse> GetAllAmazonWarehouseAddresses()
        {
            var excel = new ExcelQueryFactory(@"C:\zCush\Amazon\AmazonWarehouseAddress.xlsx");
            var amazonWarehouses = from c in excel.Worksheet<AmazonWarehouse>("AmazonWarehouses")
                                   select c;

            return amazonWarehouses.ToList();
        }
    }
}