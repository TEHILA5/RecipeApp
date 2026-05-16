using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IEmailService _emailService;
        public ContactController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        [AllowAnonymous]
        public async Task<IActionResult> Send([FromBody] ContactMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { message = "Email and message are required." });
            try
            {
                await _emailService.SendContactToAdminAsync(dto);
                return Ok(new { message = "Message sent successfully!" });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
         
        [HttpPost("reply")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reply([FromBody] AdminReplyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ToEmail) || string.IsNullOrWhiteSpace(dto.ReplyContent))
                return BadRequest(new { message = "Email and reply are required." });
            try
            {
                await _emailService.SendReplyToUserAsync(dto.ToEmail, dto.ToName, dto.Subject, dto.ReplyContent);
                return Ok(new { message = "Reply sent successfully!" });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}