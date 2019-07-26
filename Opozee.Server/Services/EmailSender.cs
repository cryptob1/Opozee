
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Web.Configuration;
using System.Net;
using System.Net.Mail;

namespace Opozee.Server.Services
{
    public static class EmailSender
    {
        public static async Task<(bool success, string errorMsg)> SendEmailAsync(string recepientName, string recepientEmail,
            string subject, string body, bool isHtml = true, bool fromContact = false)
        {
            SmtpConfig config = new SmtpConfig()
            {
                Host = WebConfigurationManager.AppSettings["SmtpHost"],
                Port = Convert.ToInt32(WebConfigurationManager.AppSettings["SmtpPort"]),
                UseSSL = Convert.ToBoolean(WebConfigurationManager.AppSettings["SmtpUseSSL"]),
                Name = WebConfigurationManager.AppSettings["SmtpName"],
                Username = WebConfigurationManager.AppSettings["SmtpEmail"],
                EmailAddress = WebConfigurationManager.AppSettings["SmtpEmail"],
                Password = WebConfigurationManager.AppSettings["SmtpPassword"]
            };

            var from = new MailboxAddress(config.Name, config.EmailAddress);
            var to = new MailboxAddress(recepientName, fromContact ? "contactus@opozee.com" : recepientEmail);

            return await SendEmailAsync(from, new MailboxAddress[] { to }, subject, body, config, isHtml);
        }

        public static async Task<(bool success, string errorMsg)> SendEmailAsync(MailboxAddress sender, MailboxAddress[] recepients, string subject, string body, SmtpConfig config = null, bool isHtml = true)
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(sender);
            message.To.AddRange(recepients);
            message.Subject = subject;
            message.Body = isHtml ? new BodyBuilder { HtmlBody = body }.ToMessageBody() : new TextPart("plain") { Text = body };

            try
            {

                //    using (var client = new MailKit.Net.Smtp.SmtpClient())
                //    {
                //        if (!config.UseSSL)
                //            client.ServerCertificateValidationCallback = (object sender2, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

                //        await client.ConnectAsync(config.Host, config.Port, config.UseSSL).ConfigureAwait(false);
                //        client.AuthenticationMechanisms.Remove("XOAUTH2");

                //        if (!string.IsNullOrWhiteSpace(config.Username))
                //            await client.AuthenticateAsync(config.Username, config.Password).ConfigureAwait(false);

                //        await client.SendAsync(message).ConfigureAwait(false);
                //        await client.DisconnectAsync(true).ConfigureAwait(false);
                //    }


                using (System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient())
                {
                    var basicCredential = new NetworkCredential(config.EmailAddress, config.Password);
                    using (MailMessage _message = new MailMessage(config.EmailAddress, recepients[0].Address))
                    {
                        var from = new MailAddress(config.EmailAddress, config.Name);

                        smtpClient.Host = config.Host;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = basicCredential;
                        smtpClient.Port = config.Port;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.EnableSsl = true;
                        //_message.From = from;
                        //_message.To.Add(recepients[0].Address);
                        _message.Subject = subject;
                        _message.IsBodyHtml = true;
                        _message.Body = body;

                        try
                        {
                            smtpClient.Send(_message);
                        }
                        catch (Exception ex)
                        {
                            return (false, ex.Message);
                        }
                    }
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }


    public class SmtpConfig
    {
        public string Host { get; set; } = "smtp.office365.com";
        public int Port { get; set; } = 587;
        public bool UseSSL { get; set; } = false;

        public string Name { get; set; } = "Opozee";
        public string Username { get; set; } = "test@gmail.com";
        public string EmailAddress { get; set; } = "test@gmail.com";
        public string Password { get; set; } = "test@gmail.com";
    }
}
