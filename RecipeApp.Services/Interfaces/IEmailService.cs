using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendNewsletterAsync(string toEmail, string toName);
    }
}
