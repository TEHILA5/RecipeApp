using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendNewsletterAsync(string toEmail, string toName);
        Task SendContactToAdminAsync(ContactMessageDto dto);
        Task SendReplyToUserAsync(string toEmail, string toName, string subject, string replyContent);
    }
}
