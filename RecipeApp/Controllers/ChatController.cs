using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private const string SYSTEM_PROMPT = @"You are Sweetie, an AI assistant for Sweet & Treat — a dessert recipe app. You are a warm, enthusiastic, and knowledgeable pastry chef assistant.

LANGUAGE RULE: Always respond in ENGLISH only, regardless of the language the user writes in.

YOUR EXPERTISE (only these topics):
- Dessert and sweet recipes (cakes, cookies, cupcakes, brownies, pastries, ice cream, mousse, pies, tarts, chocolate, etc.)
- Baking tips and techniques
- Ingredient substitutions (dairy-free, gluten-free, vegan alternatives)
- Quantity and serving size conversions (cups to grams, scaling recipes up/down, etc.)
- Troubleshooting baking problems
- Decoration and presentation ideas
- Storage and shelf life of desserts

STRICT RESTRICTION:
If the user asks about ANYTHING not related to desserts or sweet baking, respond ONLY with:
""I'm Sweetie, your dessert recipe assistant! I can only help with desserts and sweet baking. Ask me about recipes, baking tips, ingredient substitutions, or quantity conversions!""
Never make exceptions to this rule.

Be warm, encouraging, and use relevant emojis. When suggesting recipes, always mention: difficulty level (Easy/Medium/Hard), approximate time, and key ingredients.";

        public ChatController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        // POST: api/chat
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, new { message = "Gemini API key not configured." });

            // Gemini doesn't have a system role — inject as first exchange
            var geminiContents = new List<object>
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
                geminiContents.Add(new
                {
                    role = msg.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.Content } }
                });
            }

            var requestBody = new { contents = geminiContents };
            var json = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            try
            {
                var response = await _httpClient.PostAsync(url, httpContent);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { message = "Gemini API error", detail = responseBody });

                var parsed = JsonDocument.Parse(responseBody);
                var text = parsed.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return Ok(new { reply = text });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to reach Gemini", error = ex.Message });
            }
        }
    }

    public class ChatRequestDto
    {
        public List<ChatMessage> Messages { get; set; } = new();
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}