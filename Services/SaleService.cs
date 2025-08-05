using AGROPURE.Controllers;
using AGROPURE.Data;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AGROPURE.Models.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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
                DeliveryDate = createDto.DeliveryDate,
                Status = OrderStatus.Pending
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            return await GetSaleByIdAsync(sale.Id) ?? throw new InvalidOperationException("Error al crear la venta");
        }

        public async Task<SaleDto> CreateSaleFromQuoteAsync(int quoteId)
        {
            var quote = await _context.Quotes
                .Include(q => q.User)
                .Include(q => q.Product)
                .FirstOrDefaultAsync(q => q.Id == quoteId);

            if (quote == null)
            {
                throw new KeyNotFoundException("Cotización no encontrada");
            }

            if (quote.Status != QuoteStatus.Approved)
            {
                throw new InvalidOperationException("Solo se pueden crear ventas de cotizaciones aprobadas");
            }

            var sale = new Sale
            {
                UserId = quote.UserId,
                ProductId = quote.ProductId,
                QuoteId = quote.Id,
                OrderNumber = GenerateOrderNumber(),
                Quantity = quote.Quantity,
                UnitPrice = quote.UnitPrice,
                TotalAmount = quote.TotalCost,
                Status = OrderStatus.Pending,
                SaleDate = DateTime.UtcNow,
                Notes = $"Venta generada desde cotización #{quote.Id}"
            };

            _context.Sales.Add(sale);

            // Actualizar estado de la cotización
            quote.Status = QuoteStatus.Completed;

            await _context.SaveChangesAsync();

            return await GetSaleByIdAsync(sale.Id) ?? throw new InvalidOperationException("Error al crear la venta");
        }

        public async Task<SaleDto> UpdateSaleStatusAsync(int id, UpdateSaleStatusDto updateDto)
        {
            var sale = await _context.Sales
                .Include(s => s.User)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                throw new KeyNotFoundException("Venta no encontrada");
            }

            sale.Status = updateDto.Status;
            sale.Notes = updateDto.Notes;
            sale.DeliveryDate = updateDto.DeliveryDate;

            await _context.SaveChangesAsync();

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

        public async Task<List<SaleDto>> GetSalesByUserAsync(int userId)
        {
            var sales = await _context.Sales
                .Include(s => s.User)
                .Include(s => s.Product)
                .Where(s => s.UserId == userId)
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

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        }
    }
}