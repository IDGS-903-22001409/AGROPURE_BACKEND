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
        private readonly EmailService _emailService;
        private readonly ILogger<QuotesController> _logger;

        public QuotesController(QuoteService quoteService, EmailService emailService, ILogger<QuotesController> logger)
        {
            _quoteService = quoteService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("public")]
        [AllowAnonymous] // Permitir cotizaciones sin registro
        public async Task<ActionResult<QuoteDto>> CreatePublicQuote([FromBody] CreatePublicQuoteDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { field = x.Key, errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToArray();

                    return BadRequest(new { message = "Datos de cotización inválidos", errors });
                }

                var quote = await _quoteService.CreatePublicQuoteAsync(createDto);

                // Enviar notificación al administrador
                try
                {
                    await _emailService.SendQuoteNotificationToAdminAsync(quote);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error enviando email: {ex.Message}");
                }

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
                _logger.LogInformation("=== GET USER QUOTES DEBUG ===");
                _logger.LogInformation("Requested userId: {UserId}", userId);

                var currentUserId = JwtHelper.GetUserIdFromToken(User);
                var userRole = User.FindFirst("Role")?.Value;

                _logger.LogInformation("Current userId from token: {CurrentUserId}", currentUserId);
                _logger.LogInformation("User role: {UserRole}", userRole);

                // Verificar permisos
                if (userRole != "Admin" && currentUserId != userId)
                {
                    _logger.LogWarning("Access denied - user can only see own quotes");
                    return Forbid();
                }

                // Log antes de llamar al service
                _logger.LogInformation("Calling QuoteService.GetQuotesByUserAsync for userId: {UserId}", userId);

                var quotes = await _quoteService.GetQuotesByUserAsync(userId);

                _logger.LogInformation("Quotes returned: {Count}", quotes?.Count ?? 0);

                return Ok(quotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR in GetUserQuotes: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
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

        [HttpPost("{id}/approve-and-create-user")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ApproveAndCreateUser(int id)
        {
            try
            {
                await _quoteService.ApproveQuoteAndCreateUserAsync(id);
                return Ok(new { message = "Cotización aprobada y usuario creado" });
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

        // NUEVO: Método explícito para manejar OPTIONS (preflight CORS)
        [HttpOptions]
        [HttpOptions("user/{userId}")]
        [HttpOptions("{id}")]
        [HttpOptions("public")]
        [HttpOptions("{id}/status")]
        [HttpOptions("{id}/approve-and-create-user")]
        public IActionResult HandleOptions()
        {
            return Ok();
        }
    }
}