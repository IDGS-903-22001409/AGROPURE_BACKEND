using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGROPURE.Data;
using AGROPURE.Models.Enums;

namespace AGROPURE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly AgroContext _context;

        public ReportsController(AgroContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard-summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
        {
            try
            {
                var summary = new DashboardSummaryDto
                {
                    TotalUsers = await _context.Users.CountAsync(u => u.IsActive),
                    TotalProducts = await _context.Products.CountAsync(p => p.IsActive),
                    TotalQuotes = await _context.Quotes.CountAsync(),
                    PendingQuotes = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Pending),
                    ApprovedQuotes = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Approved),
                    CompletedQuotes = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Completed),
                    TotalSales = await _context.Sales.CountAsync(),
                    TotalRevenue = await _context.Sales.SumAsync(s => s.TotalAmount),
                    MonthlyRevenue = await _context.Sales
                        .Where(s => s.SaleDate >= DateTime.Now.AddDays(-30))
                        .SumAsync(s => s.TotalAmount),
                    TotalMaterials = await _context.Materials.CountAsync(m => m.IsActive),
                    TotalSuppliers = await _context.Suppliers.CountAsync(s => s.IsActive),
                    TotalReviews = await _context.Reviews.CountAsync(),
                    ApprovedReviews = await _context.Reviews.CountAsync(r => r.IsApproved)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("sales-stats")]
        public async Task<ActionResult<SalesStatsDto>> GetSalesStats()
        {
            try
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var startOfYear = new DateTime(today.Year, 1, 1);

                var stats = new SalesStatsDto
                {
                    TodaySales = await _context.Sales.CountAsync(s => s.SaleDate.Date == today),
                    TodayRevenue = await _context.Sales
                        .Where(s => s.SaleDate.Date == today)
                        .SumAsync(s => s.TotalAmount),
                    MonthSales = await _context.Sales.CountAsync(s => s.SaleDate >= startOfMonth),
                    MonthRevenue = await _context.Sales
                        .Where(s => s.SaleDate >= startOfMonth)
                        .SumAsync(s => s.TotalAmount),
                    YearSales = await _context.Sales.CountAsync(s => s.SaleDate >= startOfYear),
                    YearRevenue = await _context.Sales
                        .Where(s => s.SaleDate >= startOfYear)
                        .SumAsync(s => s.TotalAmount),
                    PendingSales = await _context.Sales.CountAsync(s => s.Status == OrderStatus.Pending),
                    CompletedSales = await _context.Sales.CountAsync(s => s.Status == OrderStatus.Delivered)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("product-stats")]
        public async Task<ActionResult<List<ProductStatsDto>>> GetProductStats()
        {
            try
            {
                var productStats = await _context.Products
                    .Include(p => p.Quotes)
                    .Include(p => p.Reviews)
                    .Where(p => p.IsActive)
                    .Select(p => new ProductStatsDto
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        BasePrice = p.BasePrice,
                        QuotesCount = p.Quotes.Count,
                        ReviewsCount = p.Reviews.Count(r => r.IsApproved),
                        AverageRating = p.Reviews.Where(r => r.IsApproved).Any() ?
                                      p.Reviews.Where(r => r.IsApproved).Average(r => r.Rating) : 0,
                        TotalRevenue = p.Quotes
                            .Where(q => q.Status == QuoteStatus.Approved || q.Status == QuoteStatus.Completed)
                            .Sum(q => q.TotalCost)
                    })
                    .OrderByDescending(p => p.QuotesCount)
                    .ToListAsync();

                return Ok(productStats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTOs simplificados
    public class DashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalQuotes { get; set; }
        public int PendingQuotes { get; set; }
        public int ApprovedQuotes { get; set; }
        public int CompletedQuotes { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalMaterials { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalReviews { get; set; }
        public int ApprovedReviews { get; set; }
    }

    public class SalesStatsDto
    {
        public int TodaySales { get; set; }
        public decimal TodayRevenue { get; set; }
        public int MonthSales { get; set; }
        public decimal MonthRevenue { get; set; }
        public int YearSales { get; set; }
        public decimal YearRevenue { get; set; }
        public int PendingSales { get; set; }
        public int CompletedSales { get; set; }
    }

    public class ProductStatsDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int QuotesCount { get; set; }
        public int ReviewsCount { get; set; }
        public double AverageRating { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}