using AGROPURE.Models.DTOs;
using AGROPURE.Services;
using Microsoft.AspNetCore.Mvc;

namespace AGROPURE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Intento de login para email: {Email}", loginDto?.Email);

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Login falló - ModelState inválido para {Email}", loginDto?.Email);
                    return BadRequest(new { message = "Datos de login inválidos", errors = ModelState });
                }

                var result = await _authService.LoginAsync(loginDto);
                if (result == null)
                {
                    _logger.LogWarning("Login falló - Credenciales inválidas para {Email}", loginDto.Email);
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                _logger.LogInformation("Login exitoso para {Email}", loginDto.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante login para {Email}", loginDto?.Email);
                return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Intento de registro para email: {Email}", registerDto?.Email);

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { field = x.Key, errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToArray();

                    _logger.LogWarning("Registro falló - ModelState inválido para {Email}", registerDto?.Email);
                    return BadRequest(new { message = "Datos de registro inválidos", errors });
                }

                // Verificar que las contraseñas coincidan
                if (registerDto.Password != registerDto.ConfirmPassword)
                {
                    _logger.LogWarning("Registro falló - Contraseñas no coinciden para {Email}", registerDto.Email);
                    return BadRequest(new { message = "Las contraseñas no coinciden" });
                }

                var user = await _authService.RegisterAsync(registerDto);

                // Hacer login automático después del registro
                var loginDto = new LoginDto
                {
                    Email = registerDto.Email,
                    Password = registerDto.Password
                };

                var loginResult = await _authService.LoginAsync(loginDto);
                _logger.LogInformation("Registro exitoso y login automático para {Email}", registerDto.Email);
                return Ok(loginResult);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registro falló - {Message} para {Email}", ex.Message, registerDto?.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante registro para {Email}", registerDto?.Email);
                return BadRequest(new { message = "Error al registrar usuario", details = ex.Message });
            }
        }

        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            try
            {
                var exists = await _authService.EmailExistsAsync(email);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando email {Email}", email);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}