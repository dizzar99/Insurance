using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
namespace EmailApp
{
    public class EmailService
    {
        private const string From = "changing.passchange@yandex.ru";
        private const string SmtpServer = "smtp.yandex.ru";
        private const int Port = 25;
        private const string Password = "DimaZarembo2909";
        public async Task SendMailAsync(string email, string subject, string message)
        {
            var emailMessage = this.CreateEmailMessage(email, subject, message);

            await this.Send(emailMessage);
        }

        private MimeMessage CreateEmailMessage(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Administrate", From));
            emailMessage.To.Add(new MailboxAddress("Address", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message };

            return emailMessage;
        }

        private async Task Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(SmtpServer, Port, false);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(From, Password);

                    client.Send(mailMessage);
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}