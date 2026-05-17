using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecipeApp.Common.DTOs;
using RecipeApp.DataContext;
using RecipeApp.Repository.Interfaces;
using RecipeApp.Repository.Repositories;
using RecipeApp.Services.Interfaces;
using RecipeApp.Services.Mapping;
using RecipeApp.Services.Services;
using RecipeApp.Services.Validators;

namespace RecipeApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5173",
                            "http://localhost:3000"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            builder.Services.AddValidations();
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RecipeApp", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    });

            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddDbContext<RecipeDbContext>(options =>
            {
                var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
                if (builder.Environment.IsProduction())
                    options.UseNpgsql(connStr);
                else
                    options.UseSqlServer(connStr);
            });

            builder.Services.AddScoped<IContext>(p => p.GetRequiredService<RecipeDbContext>());

            builder.Services.AddRepositories();
            builder.Services.AddServices();
            builder.Services.Configure<EmailSettings>(
                    builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ITextAnalysisService, TextAnalysisService>();
            builder.Services.AddHttpClient();
            builder.Services.AddSignalR();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
                if (app.Environment.IsProduction())
                {
                    var pending = db.Database.GetPendingMigrations().ToList();
                    if (pending.Any())
                    {
                        db.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS public");
                        db.Database.EnsureCreated();
                    }
                }
                else
                    db.Database.Migrate();
            }
             
            app.UseSwagger();
            app.UseSwaggerUI();
             
            app.UseCors("AllowReactApp");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<RecipeApp.Hubs.ChatHub>("/hubs/chat");
            app.MapControllers();
            app.Run();
        }
    }
}