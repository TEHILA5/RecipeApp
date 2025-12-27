using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using RecipeApp.Repository.Entities;
using Conversion = RecipeApp.Repository.Entities.Conversion;

namespace RecipeApp.DataContext
{
    public class RecipeDbContext : DbContext
    {
        public RecipeDbContext(DbContextOptions<RecipeDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Conversion> Conversions { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // RecipeIngredient - Composite Key
            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId);

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId);

            // Conversion - Self-referencing relationships
            modelBuilder.Entity<Conversion>()
                .HasOne(c => c.Ingredient1)
                .WithMany(i => i.ConversionsFrom)
                .HasForeignKey(c => c.IngredientId1)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conversion>()
                .HasOne(c => c.Ingredient2)
                .WithMany(i => i.ConversionsTo)
                .HasForeignKey(c => c.IngredientId2)
                .OnDelete(DeleteBehavior.Restrict);

            // User Email - Unique Index
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Ingredient Name - Unique Index
            modelBuilder.Entity<Ingredient>()
                .HasIndex(i => i.Name)
                .IsUnique();

            // Book - Unique constraint
            modelBuilder.Entity<Book>()
                .HasIndex(b => new { b.UserId, b.RecipeId })
                .IsUnique();

            // Additional Indexes for performance
            modelBuilder.Entity<Recipe>()
                .HasIndex(r => r.Category);

            modelBuilder.Entity<History>()
                .HasIndex(h => new { h.UserId, h.Category });

            modelBuilder.Entity<Comment>()
                .HasIndex(c => c.RecipeId);

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.UserId);
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlServer("Server=T-PC\\SQLEXPRESS;Database=ProjectDB;Trusted_Connection=True;TrustServerCertificate=True;");
        //    }
        //}
    }
}
