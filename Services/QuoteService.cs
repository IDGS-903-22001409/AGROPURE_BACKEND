using AGROPURE.Data;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;

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

        public async Task<List<QuoteDto>> GetAllQuotesAsync()
        {
            var quotes = await _context.Quotes
                .Include(q => q.User)
                .Include(q => q.Product)
                .OrderByDescending(q => q.RequestDate)
                .ToListAsync();

            return _mapper.Map<List<QuoteDto>>(quotes);
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
            quote.ExpiryDate = DateTime.UtcNow.AddDays(30); // Válida por 30 días

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            // Enviar notificación por email (opcional)
            try
            {
                await _emailService.SendQuoteRequestNotificationAsync(quote.CustomerEmail, quote.Id);
            }
            catch (Exception ex)
            {
                // Log error pero no fallar la operación
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
