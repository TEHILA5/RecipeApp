using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Repository.Entities
{
    public enum IngredientImportance
    {
        Essential = 1,
        Recommended = 2,
        Optional = 3
    }

    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        public decimal Quantity { get; set; }

        [MaxLength(50)]
        public string Unit { get; set; }

        public IngredientImportance Importance { get; set; }
    }
}
