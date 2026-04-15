using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Tadbeer.PL.Utilities;

public class EmailSetting : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSetting(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var section = _configuration.GetSection("EmailSettings");

        var host = GetValue(section, "Host", "smtp.gmail.com");
        var from = GetValue(section, "From");
        var username = GetValue(section, "Username");
        var password = GetValue(section, "Password", "EMAIL_PASSWORD", "GMAIL_APP_PASSWORD");

        if (!int.TryParse(GetValue(section, "Port", "587"), out var port))
        {
            port = 587;
        }

        if (!bool.TryParse(GetValue(section, "EnableSsl", "true"), out var enableSsl))
        {
            enableSsl = true;
        }

        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "EmailSettings are not configured. Set EmailSettings:From, EmailSettings:Username, and EmailSettings:Password (or EMAIL_PASSWORD / GMAIL_APP_PASSWORD in the hosting environment)."
            );
        }

        using var message = new MailMessage(from, email, subject, htmlMessage) { IsBodyHtml = true };
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password)
        };

        await client.SendMailAsync(message);
    }

    private string? GetValue(IConfigurationSection section, string key, params string[] fallbackKeys)
    {
        var value = section[key];
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        foreach (var fallbackKey in fallbackKeys)
        {
            value = _configuration[fallbackKey];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return value;
    }
}