using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace IdentityOrnek.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            using (var client = new SmtpClient("smtp.eu.mailgun.org", 587))
            {
                client.Credentials = new NetworkCredential("postmaster@mail.ahmetefegenc.com.tr", "92a8539b58a9f7b730845efdbfe23816-8a084751-369fb0f2");
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("postmaster@mail.ahmetefegenc.com.tr", "Recidence Hotels"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
            }
        }
    }

}

