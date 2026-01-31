using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Common.DTOs
{
    public class ConversionDto
    {
        public int Id { get; set; }
        public string Ingredient1Name { get; set; }
        public string Ingredient2Name { get; set; }
        public decimal ConversionRatio { get; set; }
        public bool IsBidirectional { get; set; }
    }

    public class ConversionCreateDto
    {
        public int IngredientId1 { get; set; }
        public int IngredientId2 { get; set; }
        public decimal ConversionRatio { get; set; }
        public bool IsBidirectional { get; set; }
    }
}
