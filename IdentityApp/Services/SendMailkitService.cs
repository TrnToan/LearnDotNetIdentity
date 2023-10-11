using Microsoft.Extensions.Options;
using MimeKit;
using System.Data;

namespace IdentityApp.Services;

public class SendMailkitService
{
    private readonly MailSettings _settings;

    public SendMailkitService(IOptions<MailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<string> SendMail(MailContent content)
    {
        var email = new MimeMessage();
        email.Sender = new MailboxAddress(_settings.DisplayName, _settings.Email);
        email.From.Add(new MailboxAddress(_settings.DisplayName, _settings.Email));
        email.To.Add(new MailboxAddress(content.To, content.To));
        email.Subject = content.Subject;

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = content.Body;
        email.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
        try
        {
            smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await smtpClient.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(_settings.Email, _settings.Password);
            await smtpClient.SendAsync(email);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        smtpClient.Disconnect(true);
        return email.Subject;
    }
}

public class MailContent
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
