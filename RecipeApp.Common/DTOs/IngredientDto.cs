using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Common.DTOs
{
    public class IngredientDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class IngredientCreateDto
    {
        public string Name { get; set; }
    }
}
