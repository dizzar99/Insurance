using Insurance.BLL.Interface.Interfaces;
using Insurance.BLL.Interface.Models.MailManagment;
using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace Insurance.BLL.Implementations
{
    public class EmailSender : IMailSender
    {
        private const string From = "changing.passchange@yandex.ru";
        private const string SmtpServer = "smtp.yandex.ru";
        private const int Port = 25;
        private const string Password = "DimaZarembo2909";
        public async Task SendMailAsync(EmailMessage message)
        {
            var emailMessage = this.CreateEmailMessage(message);

            await this.Send(emailMessage);
        }

        private MimeMessage CreateEmailMessage(EmailMessage message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Administrate", From));
            emailMessage.To.Add(new MailboxAddress("Address", message.To));
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };

            return emailMessage;
        }

        private async Task Send(MimeMessage mailMessage)
        {
            using var client = new SmtpClient();
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
