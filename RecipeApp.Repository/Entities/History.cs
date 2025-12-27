using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Repository.Entities
{
    public class History
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public RecipeCategory Category { get; set; }

        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    }
}
