using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversionController : ControllerBase
    {
        private readonly IConversionService _conversionService;

        public ConversionController(IConversionService conversionService)
        {
            _conversionService = conversionService;
        }

        // GET: api/Conversion
        [HttpGet]
        public async Task<ActionResult<List<ConversionDto>>> GetAll()
        {
            try
            {
                var conversions = await _conversionService.GetAll();
                return Ok(conversions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Conversion/:id
        [HttpGet("{id}")]
        public async Task<ActionResult<ConversionDto>> GetById(int id)
        {
            try
            {
                var conversion = await _conversionService.GetById(id);
                return Ok(conversion);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/Conversion - רק Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ConversionDto>> Create([FromBody] ConversionCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var created = await _conversionService.CreateConversion(createDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // PATCH: api/Conversion/:id - רק Admin
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ConversionDto>> Update(int id, [FromBody] ConversionUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _conversionService.UpdateConversion(id, updateDto);  // ✅
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // DELETE: api/Conversion/:id - רק Admin
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _conversionService.DeleteItem(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Conversion/find?ingredientId1=1&ingredientId2=2
        [HttpGet("find")]
        public async Task<ActionResult<ConversionDto>> FindConversion([FromQuery] int ingredientId1, [FromQuery] int ingredientId2)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var conversion = await _conversionService.FindConversion(ingredientId1, ingredientId2);
                if (conversion == null)
                    return NotFound(new { message = "Conversion not found between these ingredients." });
                return Ok(conversion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}