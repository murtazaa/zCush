using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zCush.Common.Dtos;

namespace zCush.Partners.Amazon
{
    public class AmazonWarehouse : Address
    {
        public string Identifier { get; set; }
        public string SanCode { get; set; }
        public string CartonQtySanCode { get; set; }
    }
}