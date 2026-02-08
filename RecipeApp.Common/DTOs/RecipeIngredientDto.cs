using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Common.DTOs
{
    public enum IngredientImportance
    {
        Essential = 1,
        Recommended = 2,
        Optional = 3
    }

    public class RecipeIngredientDto
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public IngredientImportance? Importance { get; set; }
    }

    public class RecipeIngredientCreateDto
    {
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public IngredientImportance Importance { get; set; }
    }
}
