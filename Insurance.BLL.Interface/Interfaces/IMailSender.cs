using Insurance.BLL.Interface.Models.MailManagment;
using System.Threading.Tasks;

namespace Insurance.BLL.Interface.Interfaces
{
    public interface IMailSender
    {
        Task SendMailAsync(EmailMessage message);
    }
}
