using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace zCush.Services.Printing
{
    public class PrintingService
    {
        public void DownloadFileAndPrint(string fileUrl, string ext)
        {
            var fileName = "shipment" + DateTime.Now.Ticks;
            using (var wc = new WebClient())
            {
                wc.DownloadFile(fileUrl, String.Format("c:/shipments/{0}.{1}", fileName, ext));
            };
        }
    }
}
