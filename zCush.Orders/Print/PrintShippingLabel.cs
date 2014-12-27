using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Web;

namespace zCush.Orders.Print
{
    public class PrintShippingLabel
    {
        public void SendToPrinter()
        {
            ProcessStartInfo info = new ProcessStartInfo("c:/test.pdf");
            info.Verb = "print";
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.Arguments = "\"HPPrinter\"";
            Process.Start(info);
            //Process p = new Process();
            //p.StartInfo = info;
            //p.Start();

            //p.WaitForInputIdle();
            //System.Threading.Thread.Sleep(3000);
            //if (false == p.CloseMainWindow())
            //    p.Kill();
        }

        public Boolean PrintPDFs(string pdfFileName, string printerName)
        {
            try
            {
                Process proc = new Process();
                //proc.StartInfo.FileName = pdfFileName;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.Verb = "print";

                //Define location of adobe reader/command line
                //switches to launch adobe in "print" mode
                proc.StartInfo.FileName =
                  @"C:\Program Files (x86)\Adobe\Reader 11.0\Reader\AcroRd32.exe";
                proc.StartInfo.Arguments = String.Format("/p /h \"{0}\" \"{1}\"", pdfFileName, printerName);
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (proc.HasExited == false)
                {
                    proc.WaitForExit(10000);
                }

                proc.EnableRaisingEvents = true;

                proc.Close();
                KillAdobe("AcroRd32");
                return true;
            }
            catch
            {
                return false;
            }
        }

        //For whatever reason, sometimes adobe likes to be a stage 5 clinger.
        //So here we kill it with fire.
        private static bool KillAdobe(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses().Where(
                         clsProcess => clsProcess.ProcessName.StartsWith(name)))
            {
                clsProcess.Kill();
                return true;
            }
            return false;
        }


        //THIS ONE WORKS!!!!
        public void PrintUsingName()
        {
            PrintDocument doc = new PrintDocument();
            doc.PrinterSettings.PrinterName = "HPPrinter";
            doc.PrintPage += new PrintPageEventHandler(PrintHandler);
            doc.Print();
        }

        private void PrintHandler(object sender, PrintPageEventArgs ppeArgs)
        {
            Font FontNormal = new Font("Verdana", 12);
            Graphics g = ppeArgs.Graphics;
            g.DrawString("Your string to print", FontNormal, Brushes.Black, 100, 100, new StringFormat());
        }

        [DllImport("Kernel32.dll")]
        static extern IntPtr CreateFile(string filename,
        [MarshalAs(UnmanagedType.U4)]FileAccess fileaccess,
        [MarshalAs(UnmanagedType.U4)]FileShare fileshare, int
        securityattributes, [MarshalAs(UnmanagedType.U4)]FileMode
        creationdisposition, int flags, IntPtr template);

        public void PrintDirect(string port, byte[] doc)
        {
            FileStream fs = new FileStream(CreateFile(port,
            FileAccess.ReadWrite,
            FileShare.ReadWrite, 0, FileMode.Create, 0, IntPtr.Zero),
            FileAccess.ReadWrite);
            fs.Write(doc, 0, doc.Length);
            fs.Close();
        }

        public void TestPrintWithInterop()
        {
            string printerPort_Lexmark = "IP_10.254.1.90"; // ports
            //mapped on the local machine to the various printers
            string printerPort_HPColor = "IP_192.168.2.3"; // the
            //lexmark will print PDF's natively, dont expect the HP
            string printerPort_PDFCreator = "PDFCreator"; // to play nicely .. but there for testing

            FileStream myFS = new FileStream("c:\\test.pdf",
            FileMode.Open, FileAccess.Read); // read in the PDF
            BinaryReader myBR = new BinaryReader(myFS);

            byte[] outData = new byte[myBR.BaseStream.Length];

            for (int x = 0; x == myBR.BaseStream.Length; x++)
            outData[x] = myBR.ReadByte();

            PrintDirect(printerPort_HPColor, outData);
        }

        public bool OneMorePrintTry()
        {
            var numberOfCopies = 1;
            var printerName = "HPD22C09 (HP Photosmart Plus B209a-m)";
            var pdfFileName = "C:/test.pdf";
            var impersonationUsername = "zCushServiceAccount";
            var impersonationPassword = "godri786";
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "-dPrinted -dNoCancel -dNOPAUSE -dBATCH -dNumCopies=" + Convert.ToString(numberOfCopies) + "  -sDEVICE=mswinpr2 -sOutputFile=%printer%\"" + printerName + "\" \"" + pdfFileName + "\" ";
            //startInfo.Arguments = "\"" + printerName + "\" \"" + pdfFileName + "\" ";

            startInfo.FileName = @"C:\Program Files (x86)\Adobe\Reader 11.0\Reader\AcroRd32.exe";
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = true;
            //startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //startInfo.UserName = impersonationUsername;
            ////startInfo.Domain = impersonationDomain;
            //SecureString ss = new SecureString();
            //for (int i = 0; i < impersonationPassword.Length; i++)
            //{
            //    ss.AppendChar(impersonationPassword[i]);
            //}
            //startInfo.Password = ss;
            Process process = null;
            try
            {
                process = Process.Start(startInfo);
                process.EnableRaisingEvents = false;
                //Logger.AddToLog("Error VS", process.StandardError.ReadToEnd());
                //Logger.AddToLog("Output VS", process.StandardOutput.ReadToEnd());
                //Logger.AddToLog(process.StartInfo.Arguments.ToString(), "VS Print Arguments");
                //Console.WriteLine(process.StandardError.ReadToEnd() + process.StandardOutput.ReadToEnd());
                //Logger.AddToLog(process.StartInfo.FileName.ToString(), "VS Print file name");
                process.WaitForExit(30000);
                if (process.HasExited == false) process.Kill();
                int exitcode = process.ExitCode;
                process.Close();
                return exitcode == 0;
            }
            catch (Exception ex)
            {
                //Logger.AddToLog(ex);
                return false;
            }
        }

        public bool PrintKarlayBhai(string file, string printer)
        {
            try
            {
                Process.Start(
                   Registry.LocalMachine.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion" +
                        @"\App Paths\AcroRd32.exe").GetValue("").ToString(),
                   string.Format("/h /t \"{0}\" \"{1}\"", file, printer));
                return true;
            }
            catch { }
            return false;
        }

    }
}