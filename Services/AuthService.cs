using AGROPURE.Data;
using AGROPURE.Helpers;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;

namespace AGROPURE.Services
{
    public class AuthService
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthService(AgroContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

            if (user == null || !PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            var jwtSecret = _configuration["JwtSettings:Secret"];
            var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"]);

            var token = JwtHelper.GenerateToken(user, jwtSecret, expiryMinutes);
            var userDto = _mapper.Map<UserDto>(user);

            return new LoginResponseDto
            {
                Token = token,
                User = userDto,
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes)
            };
        }

        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            // Verificar si el email ya existe
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new InvalidOperationException("El email ya está registrado");
            }

            var user = _mapper.Map<User>(registerDto);
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}