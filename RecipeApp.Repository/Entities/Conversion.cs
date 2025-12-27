using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Repository.Entities
{
    public class Conversion
    {
        public int Id { get; set; }

        public int IngredientId1 { get; set; }
        public Ingredient Ingredient1 { get; set; }

        public int IngredientId2 { get; set; }
        public Ingredient Ingredient2 { get; set; }

        public decimal ConversionRatio { get; set; }

        public bool IsBidirectional { get; set; }
    }
}
