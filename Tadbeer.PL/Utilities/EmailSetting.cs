using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Tadbeer.PL.Utilities;

public class EmailSetting : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential("nailasaleh2004@gmail.com", "tfnb jaie ease sywo")
        };
        return client.SendMailAsync(
            new MailMessage(from:"nailasaleh2004@gmail.com", 
                to:email,
                subject,
                htmlMessage
                ) 
                {IsBodyHtml = true}
            );
    }
}