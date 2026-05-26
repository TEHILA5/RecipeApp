using Microsoft.AspNetCore.Mvc;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsletterController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public NewsletterController(IEmailService emailService)
        {
            _emailService = emailService;
        }
         
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] NewsletterSubscribeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email is required." });

            try
            {
                await _emailService.SendNewsletterAsync(dto.Email, dto.Name ?? "Sweet Lover");
                return Ok(new { message = "Newsletter sent successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to send email: {ex.Message}" });
            }
        }
    }

}
