using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Common.DTOs;

namespace RecipeApp.Repository.Entities
{
    public class UserAction
    {
        public int Id { get; set; } 
        public int UserId { get; set; }
        public User User { get; set; } 
        public UserActionType ActionType { get; set; } 
        public int? RecipeId { get; set; }
        public Recipe? Recipe { get; set; } 
        public RecipeCategory? Category { get; set; } 
        [MaxLength(2000)]
        public string? Content { get; set; } 
        [Range(1, 5)]
        public int? Rating { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
