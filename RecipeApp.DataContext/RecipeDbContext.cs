using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;
using Conversion = RecipeApp.Repository.Entities.Conversion;

namespace RecipeApp.DataContext
{
    public class RecipeDbContext : DbContext, IContext
    {
        public RecipeDbContext(DbContextOptions<RecipeDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Conversion> Conversions { get; set; }
        public DbSet<UserAction> UserActions { get; set; }

        public async Task Save()
        {
            await SaveChangesAsync();
        }

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

            // UserAction - Indexes
            modelBuilder.Entity<UserAction>()
                .HasIndex(ua => ua.UserId);

            modelBuilder.Entity<UserAction>()
                .HasIndex(ua => ua.ActionType);

            modelBuilder.Entity<UserAction>()
                .HasIndex(ua => new { ua.UserId, ua.ActionType });

            modelBuilder.Entity<UserAction>()
                .HasIndex(ua => ua.RecipeId);

            modelBuilder.Entity<UserAction>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserActions)
                .HasForeignKey(ua => ua.UserId);

            modelBuilder.Entity<UserAction>()
                .HasOne(ua => ua.Recipe)
                .WithMany(r => r.UserActions)
                .HasForeignKey(ua => ua.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserAction>()
                .HasIndex(ua => new { ua.UserId, ua.RecipeId })
                .IsUnique()
                .HasFilter($"[{nameof(UserAction.ActionType)}] = {(int)UserActionType.Book}");

            // Recipe - Indexes
            modelBuilder.Entity<Recipe>()
                .HasIndex(r => r.Category);
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
