using System;
using System.Data;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client.Extensions.Msal;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Services.Services
{
    public class ChatService : IChatService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        private const string SYSTEM_PROMPT = @"You are Sweetie, an AI assistant for Sweet & Treat — a dessert recipe app. You are a warm, enthusiastic, and knowledgeable pastry chef assistant.

            LANGUAGE RULE: Always respond in ENGLISH only, regardless of the language the user writes in.

            YOUR EXPERTISE(only these topics):
            - Dessert and sweet recipes(cakes, cookies, cupcakes, brownies, pastries, ice cream, mousse, pies, tarts, chocolate, etc.)
            - Baking tips and techniques
            - Ingredient substitutions(dairy-free, gluten-free, vegan alternatives)
            - Quantity and serving size conversions(cups to grams, scaling recipes up/down, etc.)
            - Troubleshooting baking problems
            - Decoration and presentation ideas
            - Storage and shelf life of desserts

            STRICT RESTRICTION:
            If the user asks about ANYTHING not related to desserts or sweet baking, respond ONLY with:
            ""I'm Sweetie, your dessert recipe assistant! I can only help with desserts and sweet baking. Ask me about recipes, baking tips, ingredient substitutions, or quantity conversions!""
            Never make exceptions to this rule.

            Be warm, encouraging, and use relevant emojis.When suggesting recipes, always mention: difficulty level(Easy/Medium/Hard), approximate time, and key ingredients.";


        public ChatService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _config = configuration;
            _http = httpClientFactory.CreateClient();
        }

        public async Task<string> GetReplyAsync(ChatRequestDto request)
        {
            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Gemini API key not configured.");

            var contents = new List<object>
            {
                new {
                    role = "user",
                    parts = new[] { new { text = SYSTEM_PROMPT } }
                },
                new {
                    role = "model",
                    parts = new[] { new { text = "Understood! I am Sweetie, your Sweet & Treat dessert assistant. Ready to help with recipes, baking tips, and more! 🍰" } }
                }
            };

            foreach (var msg in request.Messages)
            {
                contents.Add(new
                {
                    role = msg.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.Content } }
                });
            }

            var json = JsonSerializer.Serialize(new { contents });
            var body = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var response = await _http.PostAsync(url, body);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini API error: {raw}", null, response.StatusCode);

            var text = JsonDocument.Parse(raw)
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? string.Empty;
        }
    }
}