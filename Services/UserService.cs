using AGROPURE.Data;
using AGROPURE.Helpers;
using AGROPURE.Models.DTOs;
using AutoMapper;

namespace AGROPURE.Services
{
    public class UserService
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;

        public UserService(AgroContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive)
            {
                throw new KeyNotFoundException("Usuario no encontrado");
            }

            _mapper.Map(updateDto, user);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            if (!PasswordHelper.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return false;
            }

            user.PasswordHash = PasswordHelper.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}