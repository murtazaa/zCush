using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace zCush.Services.Printing
{
    public class PrintingService
    {
        public void DownloadFileAndPrint(string fileUrl)
        {
            var fileName = "shipment" + DateTime.Now.Ticks;
            using (var wc = new WebClient())
            {
                wc.DownloadFile(fileUrl, String.Format("c:/shipments/{0}.png", fileName));
                printLabel(fileName);
            };
        }

        private void printLabel(string fileName)
        {
            var pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = "HPPrinter";
            pd.PrintPage += (sender, args) =>
            {
                var i = Image.FromFile(String.Format("c:/shipments/{0}.png", fileName));
                var p = new Point(100, 100);
                args.Graphics.DrawImage(i, 10, 10, i.Width, i.Height);
            };

            using (var wic = WindowsIdentity.Impersonate(IntPtr.Zero))
            {
                pd.Print();
            }                  
        }
    }
}
