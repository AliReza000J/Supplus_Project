namespace _0_Framework.Application
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }

}
