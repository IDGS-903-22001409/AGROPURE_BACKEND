using AGROPURE.Helpers;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AGROPURE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGROPURE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotesController : ControllerBase
    {
        private readonly QuoteService _quoteService;

        public QuotesController(QuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<QuoteDto>>> GetAllQuotes()
        {
            try
            {
                var quotes = await _quoteService.GetAllQuotesAsync();
                return Ok(quotes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<List<QuoteDto>>> GetUserQuotes(int userId)
        {
            try
            {
                // Verificar que el usuario solo pueda ver sus propias cotizaciones (a menos que sea admin)
                var currentUserId = JwtHelper.GetUserIdFromToken(User);
                var userRole = User.FindFirst("Role")?.Value;

                if (userRole != "Admin" && currentUserId != userId)
                {
                    return Forbid();
                }

                var quotes = await _quoteService.GetQuotesByUserAsync(userId);
                return Ok(quotes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<QuoteDto>> GetQuote(int id)
        {
            try
            {
                var quote = await _quoteService.GetQuoteByIdAsync(id);
                if (quote == null)
                {
                    return NotFound(new { message = "Cotización no encontrada" });
                }

                // Verificar permisos
                var currentUserId = JwtHelper.GetUserIdFromToken(User);
                var userRole = User.FindFirst("Role")?.Value;

                if (userRole != "Admin" && currentUserId != quote.UserId)
                {
                    return Forbid();
                }

                return Ok(quote);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<QuoteDto>> CreateQuote([FromBody] CreateQuoteDto createDto)
        {
            try
            {

                // DEBUG: Ver qué datos llegan
                Console.WriteLine($"=== QUOTE DEBUG ===");
                Console.WriteLine($"ProductId: {createDto?.ProductId}");
                Console.WriteLine($"Quantity: {createDto?.Quantity}");
                Console.WriteLine($"CustomerNotes: {createDto?.Notes}");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { field = x.Key, errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToArray();

                    return BadRequest(new { message = "Datos de cotización inválidos", errors });
                }

                var userId = JwtHelper.GetUserIdFromToken(User);
                var quote = await _quoteService.CreateQuoteAsync(createDto, userId);

                return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, quote);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<QuoteDto>> UpdateQuoteStatus(int id, [FromBody] UpdateQuoteStatusDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de actualización inválidos", errors = ModelState });
                }

                var adminUserId = JwtHelper.GetUserIdFromToken(User);
                var quote = await _quoteService.UpdateQuoteStatusAsync(id, updateDto, adminUserId);

                return Ok(quote);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteQuote(int id)
        {
            try
            {
                var result = await _quoteService.DeleteQuoteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Cotización no encontrada" });
                }

                return Ok(new { message = "Cotización eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}