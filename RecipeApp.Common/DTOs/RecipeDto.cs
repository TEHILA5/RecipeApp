using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Common.DTOs
{
    public enum RecipeCategory
    {
        Sweats,
        Cakes, Cupcakes, Cheesecakes, BundtCakes,
        Brownies, Cookies, Bars,
        IceCream, Mousse, Puddings, Panna, Tiramisu, FrozenDesserts,
        Pies, Tarts, Crumbles, FruitSalads,
        Pastries, Donuts, Churros, Crepes, Waffles,
        NoBakeCakes, Truffles, EnergyBalls,
        SoufleeAndCustard, MilkDesserts, JellyAndGelatin, TraditionalDesserts
    }

    public class RecipeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public RecipeCategory? Category { get; set; }
        public string? Instructions { get; set; }
        public string? ArrImage { get; set; }
        public int? Servings { get; set; }
        public int? Level { get; set; }
        public int? PrepTime { get; set; }
        public int? TotalTime { get; set; }
        public List<RecipeIngredientDto>? Ingredients { get; set; }
        public double? AverageRating { get; set; }
        public int? CommentCount { get; set; } 
        public List<string>? Tags { get; set; }
    }

    public class RecipeCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public RecipeCategory Category { get; set; }
        public string Instructions { get; set; }
        public string ArrImage { get; set; }
        public int Servings { get; set; }
        public int Level { get; set; }
        public int PrepTime { get; set; }
        public int TotalTime { get; set; }
        public List<RecipeIngredientCreateDto> Ingredients { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class RecipeUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public RecipeCategory? Category { get; set; }
        public string? Instructions { get; set; }
        public string? ArrImage { get; set; }
        public int? Servings { get; set; }
        public int? Level { get; set; }
        public int? PrepTime { get; set; }
        public int? TotalTime { get; set; }
        public List<RecipeIngredientCreateDto>? Ingredients { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class RecipeSearchDto
    {
        public RecipeCategory Category { get; set; }
        public int? MaxPrepTime { get; set; }
        public int? MaxLevel { get; set; }
        public List<string> AvailableIngredients { get; set; }
    }
}