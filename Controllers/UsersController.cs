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
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                // Verificar que el usuario solo pueda ver su propio perfil (a menos que sea admin)
                var currentUserId = JwtHelper.GetUserIdFromToken(User);
                var userRole = User.FindFirst("Role")?.Value;

                if (userRole != "Admin" && currentUserId != id)
                {
                    return Forbid();
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetCurrentUserProfile()
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(User);
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
        {
            try
            {
                // Verificar que el usuario solo pueda actualizar su propio perfil (a menos que sea admin)
                var currentUserId = JwtHelper.GetUserIdFromToken(User);
                var userRole = User.FindFirst("Role")?.Value;

                if (userRole != "Admin" && currentUserId != id)
                {
                    return Forbid();
                }

                var user = await _userService.UpdateUserAsync(id, updateDto);
                return Ok(user);
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

        [HttpPost("{id}/change-password")]
        public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                // Verificar que el usuario solo pueda cambiar su propia contraseña
                var currentUserId = JwtHelper.GetUserIdFromToken(User);
                if (currentUserId != id)
                {
                    return Forbid();
                }

                var result = await _userService.ChangePasswordAsync(id, changePasswordDto);
                if (!result)
                {
                    return BadRequest(new { message = "Contraseña actual incorrecta" });
                }

                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeactivateUser(int id)
        {
            try
            {
                var result = await _userService.DeactivateUserAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(new { message = "Usuario desactivado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}