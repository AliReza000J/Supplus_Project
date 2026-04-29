using _0_Framework.Application;
using Microsoft.Extensions.Configuration;
//using MailKit.Net.Smtp;
//using MailKit.Security;
//using MimeKit;

namespace _0_Framework.Infrastructure
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            //var email = new MimeMessage();

            //var senderName = _configuration["SmtpConfig:SenderName"];
            //var senderEmail = _configuration["SmtpConfig:Username"];

            //email.From.Add(new MailboxAddress(senderName, senderEmail));
            //email.To.Add(MailboxAddress.Parse(to));
            //email.Subject = subject;

            //// ساخت بدنه ایمیل به صورت HTML
            //var builder = new BodyBuilder { HtmlBody = htmlBody };
            //email.Body = builder.MessagePart;

            //using var smtp = new SmtpClient();
            //try
            //{
            //    var host = _configuration["SmtpConfig:Host"];
            //    var port = int.Parse(_configuration["SmtpConfig:Port"]);
            //    var password = _configuration["SmtpConfig:Password"];

            //    // اتصال امن به سرور
            //    await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            //    await smtp.AuthenticateAsync(senderEmail, password);

            //    // ارسال
            //    await smtp.SendAsync(email);
            //}
            //finally
            //{
            //    // قطع اتصال
            //    await smtp.DisconnectAsync(true);
            //}
        }
    }

}
