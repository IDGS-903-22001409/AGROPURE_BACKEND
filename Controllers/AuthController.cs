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

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de login inválidos", errors = ModelState });
                }

                var result = await _authService.LoginAsync(loginDto);
                if (result == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {

                Console.WriteLine($"=== REGISTER DEBUG ===");
                Console.WriteLine($"Email: {registerDto?.Email}");
                Console.WriteLine($"FirstName: {registerDto?.FirstName}");
                Console.WriteLine($"LastName: {registerDto?.LastName}");
                Console.WriteLine($"Password length: {registerDto?.Password?.Length}");
                Console.WriteLine($"ConfirmPassword length: {registerDto?.ConfirmPassword?.Length}");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { field = x.Key, errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToArray();

                    return BadRequest(new { message = "Datos de registro inválidos", errors });
                }

                // Verificar que las contraseñas coincidan
                if (registerDto.Password != registerDto.ConfirmPassword)
                {
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
                return Ok(loginResult);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al registrar usuario", details = ex.Message });
            }
        }

        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            var exists = await _authService.EmailExistsAsync(email);
            return Ok(new { exists });
        }
    }
}