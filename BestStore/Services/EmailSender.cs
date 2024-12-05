using System.Net;
using System.Net.Mail;

namespace BestStore.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            //var fromEmail = "Phillmon8917@outlook.com";
             var fromEmail = "admin@123";

            var emailPass = "admin@123";

            var client = new SmtpClient("smtp.gmail.com``", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail.Trim(), emailPass.Trim()),
                UseDefaultCredentials = false
            };

            return client.SendMailAsync(new MailMessage(from: fromEmail, to: toEmail, subject, message));
        }
    }
}
