using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RecipeApp.Common.DTOs
{
    public enum RecipeCategory
    {
        Sweats,  //ערך ברירת מחדל
        // עוגות ומאפים
        Cakes,                    // עוגות שכבות, עוגות יום הולדת
        Cupcakes,                 // קאפקייקס, מאפינס מתוקים
        Cheesecakes,              // עוגות גבינה
        BundtCakes,               // עוגות בחושה
        Brownies,                 // בראוניז
        Cookies,                  // עוגיות
        Bars,                     // חיתוכיות, בלונדיז

        // קינוחים קרים
        IceCream,                 // גלידה ביתית
        Mousse,                   // מוס שוקולד, מוס פירות
        Puddings,                 // פודינג, קרם
        Panna,                    // פנקוטה
        Tiramisu,                 // טירמיסו וקינוחי קפה
        FrozenDesserts,           // עוגות גלידה, סמיפרדו

        // קינוחי פירות
        Pies,                     // פאי תפוחים, פאי דלעת
        Tarts,                    // טארט לימון, טארט פירות
        Crumbles,                 // קרמבל, כריספ
        FruitSalads,              // סלט פירות, פירות ברוטב

        // מאפים מתוקים
        Pastries,                 // דניש, קרואסון שוקולד, שטרודל
        Donuts,                   // סופגניות, דונאטס
        Churros,                  // צ'ורוס, מטבעות
        Crepes,                   // קרפים מתוקים, בלינצ'ס
        Waffles,                  // וופלים, פנקייק מתוקים

        // ללא אפייה
        NoBakeCakes,              // עוגות ללא אפייה,כמו עוגת ביסקוויטים
        Truffles,                 // טראפלס שוקולד
        EnergyBalls,              // כדורי אנרגיה, כדורי תמרים

        // מיוחדים
        SoufleeAndCustard,        // סופלה, קרם ברולה
        MilkDesserts,             // מלבי, קינוחי חלב
        JellyAndGelatin,          // ג'לי, קינוחי ג'לטין
        TraditionalDesserts       // קינוחים מסורתיים (קוגל, בורקס שוקולד)
    }
    public class RecipeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public RecipeCategory? Category { get; set; }
        public string? Instructions { get; set; }
        public string? ImageUrl { get; set; }  
        public int? Servings { get; set; }
        public int? Level { get; set; }
        public int? PrepTime { get; set; }
        public int? TotalTime { get; set; }
        public List<RecipeIngredientDto>? Ingredients { get; set; }
        public double? AverageRating { get; set; }
        public int? CommentCount { get; set; }
    }

    //רק למנהל
    public class RecipeCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public RecipeCategory Category { get; set; }
        public string Instructions { get; set; }
        public string ImageUrl { get; set; } 
        public int Servings { get; set; }
        public int Level { get; set; }
        public int PrepTime { get; set; }
        public int TotalTime { get; set; }
        public List<RecipeIngredientCreateDto> Ingredients { get; set; }
    }

    public class RecipeUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public RecipeCategory? Category { get; set; }
        public string? Instructions { get; set; }
        public string? ImageUrl { get; set; }
        public int? Servings { get; set; }
        public int? Level { get; set; }
        public int? PrepTime { get; set; }
        public int? TotalTime { get; set; }
        public List<RecipeIngredientCreateDto>? Ingredients { get; set; }
    }

    public class RecipeSearchDto
    {
        public RecipeCategory Category { get; set; }
        public int? MaxPrepTime { get; set; }
        public int? MaxLevel { get; set; }
        public List<string> AvailableIngredients { get; set; }
    }

    /// <summary>
    /// DTO ליצירת מתכון עם תמונה - תומך בשתי אופציות:
    /// 1. העלאת קובץ תמונה (ImageFile)
    /// 2. קישור לתמונה חיצונית (ImageUrl)
    /// </summary>
    public class RecipeCreateFormDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public RecipeCategory Category { get; set; }
        public string Instructions { get; set; }

        // אופציה 1: העלאת קובץ תמונה
        public IFormFile? ImageFile { get; set; }

        // אופציה 2: קישור לתמונה חיצונית
        public string? ImageUrl { get; set; }

        public int Servings { get; set; }
        public int Level { get; set; }
        public int PrepTime { get; set; }
        public int TotalTime { get; set; }

        // JSON string של מערך מרכיבים (כי multipart/form-data לא תומך באובייקטים מקוננים)
        public string? IngredientsJson { get; set; }
    }

    /// <summary>
    /// DTO לעדכון מתכון עם תמונה - כל השדות אופציונליים
    /// </summary>
    public class RecipeUpdateFormDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public RecipeCategory? Category { get; set; }
        public string? Instructions { get; set; }

        // אופציה 1: העלאת קובץ תמונה חדש
        public IFormFile? ImageFile { get; set; }

        // אופציה 2: קישור לתמונה חדש
        public string? ImageUrl { get; set; }

        public int? Servings { get; set; }
        public int? Level { get; set; }
        public int? PrepTime { get; set; }
        public int? TotalTime { get; set; }

        // JSON string של מערך מרכיבים
        public string? IngredientsJson { get; set; }
    }
}
