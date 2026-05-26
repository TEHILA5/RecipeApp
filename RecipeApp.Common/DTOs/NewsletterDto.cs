using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Common.DTOs
{ 
    public class NewsletterSubscribeDto
    {
        public string Email { get; set; } = "";
        public string? Name { get; set; }
    }
}
