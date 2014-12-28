using ActiveUp.Net.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zCush.Services.Email
{
    public class zCushOrderEmail : Message { };

    public class EmailService
    {
        public List<zCushOrderEmail> GetAllEmailsOn(DateTime date)
        {
            var ImapServer = "imap.secureserver.net";
            var userName = "orders@zcush.com";
            var password = "godri786";
            var orderMails = new List<zCushOrderEmail>();

            // We create Imap client
            var imap = new Imap4Client();

            try
            {
                // We connect to the imap4 server
                imap.Connect(ImapServer);

                // Login to mail box
                imap.Login(userName, password);

                Mailbox inbox = imap.SelectMailbox("inbox");
                var msgCount = inbox.MessageCount;

                if (msgCount > 0)
                {
                    var newMessage = inbox.Fetch.MessageObject(msgCount--);

                    while (newMessage.ReceivedDate > date)
                    {
                        var zOrderEmail = new zCushOrderEmail
                        {
                            Subject = newMessage.Subject,
                            BodyText = newMessage.BodyText
                        };
                        orderMails.Add(zOrderEmail);
                        newMessage = inbox.Fetch.MessageObject(msgCount--);
                    }
                }

                return orderMails;
            }

            catch (Imap4Exception iex)
            {
                //this.AddLogEntry(string.Format("Imap4 Error: {0}", iex.Message));
            }

            catch (Exception ex)
            {
                //this.AddLogEntry(string.Format("Failed: {0}", ex.Message));
            }

            finally
            {
                if (imap.IsConnected)
                {
                    imap.Disconnect();
                }
            }

            return orderMails;
        }
    }
}
