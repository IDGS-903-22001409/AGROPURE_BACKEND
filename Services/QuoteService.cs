using Microsoft.EntityFrameworkCore;
using AGROPURE.Models.Enums;
using AGROPURE.Data;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;
using AGROPURE.Helpers;

namespace AGROPURE.Services
{
    public class QuoteService
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;
        private readonly CostingService _costingService;
        private readonly EmailService _emailService;

        public QuoteService(AgroContext context, IMapper mapper, CostingService costingService, EmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _costingService = costingService;
            _emailService = emailService;
        }

        public async Task<QuoteDto> CreatePublicQuoteAsync(CreatePublicQuoteDto createDto)
        {
            // Verificar que el producto existe
            var product = await _context.Products.FindAsync(createDto.ProductId);
            if (product == null || !product.IsActive)
            {
                throw new KeyNotFoundException("Producto no encontrado");
            }

            // Calcular precios usando el servicio de costeo
            var costCalculation = await _costingService.CalculateProductCostAsync(createDto.ProductId, createDto.Quantity);

            var quote = new Quote
            {
                UserId = null, // Cotización pública sin usuario
                ProductId = createDto.ProductId,
                CustomerName = createDto.CustomerName,
                CustomerEmail = createDto.CustomerEmail,
                CustomerPhone = createDto.CustomerPhone,
                CustomerAddress = createDto.CustomerAddress,
                CustomerCompany = createDto.CustomerCompany,
                Quantity = createDto.Quantity,
                UnitPrice = costCalculation.UnitPrice,
                TotalCost = costCalculation.TotalCost,
                Notes = createDto.Notes,
                RequestDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = QuoteStatus.Pending,
                IsPublicQuote = true
            };

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return await GetQuoteByIdAsync(quote.Id) ?? throw new InvalidOperationException("Error al crear la cotización");
        }

        public async Task ApproveQuoteAndCreateUserAsync(int quoteId)
        {
            var quote = await _context.Quotes
                .Include(q => q.Product)
                .FirstOrDefaultAsync(q => q.Id == quoteId);

            if (quote == null)
            {
                throw new KeyNotFoundException("Cotización no encontrada");
            }

            // Verificar si ya existe un usuario con ese email
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == quote.CustomerEmail);

            if (existingUser == null)
            {
                // Crear nuevo usuario
                var tempPassword = GenerateRandomPassword();
                var nameParts = quote.CustomerName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var newUser = new User
                {
                    FirstName = nameParts.FirstOrDefault() ?? quote.CustomerName,
                    LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "",
                    Email = quote.CustomerEmail,
                    PasswordHash = PasswordHelper.HashPassword(tempPassword),
                    Phone = quote.CustomerPhone,
                    Company = quote.CustomerCompany,
                    Address = quote.CustomerAddress,
                    Role = UserRole.Customer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Actualizar la cotización con el nuevo usuario
                quote.UserId = newUser.Id;
                quote.Status = QuoteStatus.Approved;
                quote.ResponseDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Enviar email de bienvenida con credenciales
                try
                {
                    await _emailService.SendWelcomeEmailAsync(newUser.Email, quote.CustomerName, tempPassword);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error enviando email de bienvenida: {ex.Message}");
                }
            }
            else
            {
                // Usuario ya existe, solo aprobar cotización
                quote.UserId = existingUser.Id;
                quote.Status = QuoteStatus.Approved;
                quote.ResponseDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<List<QuoteDto>> GetAllQuotesAsync()
        {
            var quotes = await _context.Quotes
                .Include(q => q.User)
                .Include(q => q.Product)
                .OrderByDescending(q => q.RequestDate)
                .ToListAsync();

            return quotes.Select(q => new QuoteDto
            {
                Id = q.Id,
                UserId = q.UserId,
                ProductId = q.ProductId,
                CustomerName = q.CustomerName,
                CustomerEmail = q.CustomerEmail,
                CustomerPhone = q.CustomerPhone,
                CustomerAddress = q.CustomerAddress,
                CustomerCompany = q.CustomerCompany,
                Quantity = q.Quantity,
                UnitPrice = q.UnitPrice,
                TotalCost = q.TotalCost,
                Status = q.Status,
                Notes = q.Notes,
                AdminNotes = q.AdminNotes,
                RequestDate = q.RequestDate,
                ResponseDate = q.ResponseDate,
                ExpiryDate = q.ExpiryDate,
                IsPublicQuote = q.IsPublicQuote,
                ProductName = q.Product.Name,
                UserFullName = q.User != null ? $"{q.User.FirstName} {q.User.LastName}" : null
            }).ToList();
        }

        public async Task<List<QuoteDto>> GetQuotesByUserAsync(int userId)
        {
            var quotes = await _context.Quotes
                .Include(q => q.User)
                .Include(q => q.Product)
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.RequestDate)
                .ToListAsync();

            return _mapper.Map<List<QuoteDto>>(quotes);
        }

        public async Task<QuoteDto?> GetQuoteByIdAsync(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.User)
                .Include(q => q.Product)
                .FirstOrDefaultAsync(q => q.Id == id);

            return quote != null ? _mapper.Map<QuoteDto>(quote) : null;
        }

        public async Task<QuoteDto> CreateQuoteAsync(CreateQuoteDto createDto, int userId)
        {
            // Verificar que el producto existe
            var product = await _context.Products.FindAsync(createDto.ProductId);
            if (product == null || !product.IsActive)
            {
                throw new KeyNotFoundException("Producto no encontrado");
            }

            // Calcular precios usando el servicio de costeo
            var costCalculation = await _costingService.CalculateProductCostAsync(createDto.ProductId, createDto.Quantity);

            var quote = _mapper.Map<Quote>(createDto);
            quote.UserId = userId;
            quote.UnitPrice = costCalculation.UnitPrice;
            quote.TotalCost = costCalculation.TotalCost;
            quote.RequestDate = DateTime.UtcNow;
            quote.ExpiryDate = DateTime.UtcNow.AddDays(30);
            quote.IsPublicQuote = false;

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            // Enviar notificación por email (opcional)
            try
            {
                await _emailService.SendQuoteRequestNotificationAsync(quote.CustomerEmail, quote.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando email: {ex.Message}");
            }

            return await GetQuoteByIdAsync(quote.Id) ?? throw new InvalidOperationException("Error al crear la cotización");
        }

        public async Task<QuoteDto> UpdateQuoteStatusAsync(int id, UpdateQuoteStatusDto updateDto, int adminUserId)
        {
            var quote = await _context.Quotes
                .Include(q => q.User)
                .Include(q => q.Product)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
            {
                throw new KeyNotFoundException("Cotización no encontrada");
            }

            quote.Status = updateDto.Status;
            quote.AdminNotes = updateDto.AdminNotes;
            quote.ResponseDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Enviar notificación por email
            try
            {
                await _emailService.SendQuoteStatusUpdateAsync(quote.CustomerEmail, quote.Id, updateDto.Status);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando email: {ex.Message}");
            }

            return _mapper.Map<QuoteDto>(quote);
        }

        public async Task<bool> DeleteQuoteAsync(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null)
            {
                return false;
            }

            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
