using AGROPURE.Data;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;

namespace AGROPURE.Services
{
    public class SaleService
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;

        public SaleService(AgroContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<SaleDto>> GetAllSalesAsync()
        {
            var sales = await _context.Sales
                .Include(s => s.User)
                .Include(s => s.Product)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return sales.Select(s => new SaleDto
            {
                Id = s.Id,
                UserId = s.UserId,
                ProductId = s.ProductId,
                QuoteId = s.QuoteId,
                OrderNumber = s.OrderNumber,
                Quantity = s.Quantity,
                UnitPrice = s.UnitPrice,
                TotalAmount = s.TotalAmount,
                Status = s.Status.ToString(),
                Notes = s.Notes,
                SaleDate = s.SaleDate,
                DeliveryDate = s.DeliveryDate,
                CustomerName = $"{s.User.FirstName} {s.User.LastName}",
                ProductName = s.Product.Name
            }).ToList();
        }

        public async Task<SaleDto?> GetSaleByIdAsync(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.User)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return null;

            return new SaleDto
            {
                Id = sale.Id,
                UserId = sale.UserId,
                ProductId = sale.ProductId,
                QuoteId = sale.QuoteId,
                OrderNumber = sale.OrderNumber,
                Quantity = sale.Quantity,
                UnitPrice = sale.UnitPrice,
                TotalAmount = sale.TotalAmount,
                Status = sale.Status.ToString(),
                Notes = sale.Notes,
                SaleDate = sale.SaleDate,
                DeliveryDate = sale.DeliveryDate,
                CustomerName = $"{sale.User.FirstName} {sale.User.LastName}",
                ProductName = sale.Product.Name
            };
        }

        public async Task<SaleDto> CreateSaleAsync(CreateSaleDto createDto)
        {
            var sale = new Sale
            {
                UserId = createDto.UserId,
                ProductId = createDto.ProductId,
                QuoteId = createDto.QuoteId,
                OrderNumber = GenerateOrderNumber(),
                Quantity = createDto.Quantity,
                UnitPrice = createDto.UnitPrice,
                TotalAmount = createDto.UnitPrice * createDto.Quantity,
                Notes = createDto.Notes,
                SaleDate = DateTime.UtcNow,
                DeliveryDate = createDto.DeliveryDate
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            return await GetSaleByIdAsync(sale.Id) ?? throw new InvalidOperationException("Error al crear la venta");
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        }
    }
}