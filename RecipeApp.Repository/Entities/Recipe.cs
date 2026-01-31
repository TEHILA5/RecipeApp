using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Repository.Entities
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public RecipeCategory Category { get; set; }

        [Required]
        public string Instructions { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public int Servings { get; set; }

        [Range(1, 5)]
        public int Level { get; set; }

        public int PrepTime { get; set; }

        public int TotalTime { get; set; }

        // Navigation Properties
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
        public ICollection<UserAction> UserActions { get; set; }
    }
}
