using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace RecipeApp.Common.DTOs
{
    public enum UserActionType
    {
        Comment = 1,
        History = 2,
        Book = 3
    }
    public class UserActionDto
    {
        public int Id { get; set; }

        public UserActionType ActionType { get; set; } 

        public int? RecipeId { get; set; }
        public string? RecipeName { get; set; }
        public string? RecipeImageUrl { get; set; }

        public RecipeCategory? Category { get; set; }

        // Comment
        public string? UserName { get; set; }
        public string? Content { get; set; }
        public int? Rating { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    public class BookCreateDto
    { 
        public int RecipeId { get; set; }
    }

    public class CommentCreateDto
    { 
        public int RecipeId { get; set; }

        public string Content { get; set; }
        public int Rating { get; set; }
    }

    public class HistoryCreateDto
    { 
        public RecipeCategory Category { get; set; }
    }

    public class UserPreferencesDto
    {
        public RecipeCategory FavoriteCategory { get; set; }
        public List<CategoryStatsDto> CategoryStats { get; set; }
    }

    public class CategoryStatsDto
    {
        public RecipeCategory Category { get; set; }
        public int SearchCount { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class WeeklyCategoryStatsDto
    {
        public string Week { get; set; } = ""; // "2024-W03"
        public string WeekLabel { get; set; } = ""; // "Jan 15–21"
        public RecipeCategory Category { get; set; }
        public string CategoryName { get; set; } = "";
        public int ViewCount { get; set; }
    }
}
