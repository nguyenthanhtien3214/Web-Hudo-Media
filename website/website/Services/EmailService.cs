using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace website.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            using (var client = new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"])))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]);
                client.EnableSsl = true; // Bật SSL

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings["FromEmail"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
