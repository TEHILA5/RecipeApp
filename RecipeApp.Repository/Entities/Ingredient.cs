using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace RecipeApp.Repository.Entities
{
    public class Ingredient
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // Navigation Properties
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
        public ICollection<Conversion> ConversionsFrom { get; set; }
        public ICollection<Conversion> ConversionsTo { get; set; }
    }
}
