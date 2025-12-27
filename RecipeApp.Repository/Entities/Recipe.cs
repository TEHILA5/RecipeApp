using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Repository.Entities
{
    public enum RecipeCategory
    {
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
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Book> SavedByUsers { get; set; }
    }
}
