using Microsoft.EntityFrameworkCore;
using AGROPURE.Models.Enums;
using AGROPURE.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGROPURE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly AgroContext _context;

        public DashboardController(AgroContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
                var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
                var totalQuotes = await _context.Quotes.CountAsync();
                var pendingQuotes = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Pending);
                var totalSales = await _context.Sales.CountAsync();
                var monthlyRevenue = await _context.Sales
                    .Where(s => s.SaleDate >= DateTime.Now.AddDays(-30))
                    .SumAsync(s => s.TotalAmount);

                var recentQuotes = await _context.Quotes
                    .Include(q => q.User)
                    .Include(q => q.Product)
                    .OrderByDescending(q => q.RequestDate)
                    .Take(5)
                    .Select(q => new RecentQuoteDto
                    {
                        Id = q.Id,
                        CustomerName = q.CustomerName,
                        ProductName = q.Product.Name,
                        Quantity = q.Quantity,
                        TotalCost = q.TotalCost,
                        Status = q.Status.ToString(),
                        RequestDate = q.RequestDate
                    })
                    .ToListAsync();

                var monthlyQuoteStats = await _context.Quotes
                    .Where(q => q.RequestDate >= DateTime.Now.AddDays(-180)) // Últimos 6 meses
                    .GroupBy(q => new { q.RequestDate.Year, q.RequestDate.Month })
                    .Select(g => new MonthlyStatDto
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:00}",
                        Count = g.Count(),
                        Value = g.Sum(q => q.TotalCost)
                    })
                    .OrderBy(s => s.Month)
                    .ToListAsync();
                
                var topProducts = await _context.Quotes
                    .Include(q => q.Product)
                    .Where(q => q.RequestDate >= DateTime.Now.AddDays(-90)) // Últimos 3 meses
                    .GroupBy(q => new { q.ProductId, q.Product.Name })
                    .Select(g => new ProductStatDto
                    {
                        ProductName = g.Key.Name,
                        QuotesCount = g.Count(),
                        TotalRevenue = g.Where(q => q.Status == QuoteStatus.Approved || q.Status == QuoteStatus.Completed)
                                      .Sum(q => q.TotalCost)
                    })
                    .OrderByDescending(p => p.QuotesCount)
                    .Take(5)
                    .ToListAsync();

                return Ok(new DashboardStatsDto
                {
                    TotalUsers = totalUsers,
                    TotalProducts = totalProducts,
                    TotalQuotes = totalQuotes,
                    PendingQuotes = pendingQuotes,
                    TotalSales = totalSales,
                    MonthlyRevenue = monthlyRevenue,
                    RecentQuotes = recentQuotes,
                    MonthlyQuoteStats = monthlyQuoteStats,
                    TopProducts = topProducts
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("sales-chart")]
        public async Task<ActionResult<List<MonthlyStatDto>>> GetSalesChart()
        {
            try
            {
                var salesData = await _context.Sales
                    .Where(s => s.SaleDate >= DateTime.Now.AddDays(-365)) // Último año
                    .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                    .Select(g => new MonthlyStatDto
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:00}",
                        Count = g.Count(),
                        Value = g.Sum(s => s.TotalAmount)
                    })
                    .OrderBy(s => s.Month)
                    .ToListAsync();

                return Ok(salesData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpGet("user-stats")]
        public async Task<ActionResult<UserStatsDto>> GetUserStats()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var adminUsers = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
                var customerUsers = await _context.Users.CountAsync(u => u.Role == UserRole.Customer);
                var newUsersThisMonth = await _context.Users
                    .CountAsync(u => u.CreatedAt >= DateTime.Now.AddDays(-30));

                return Ok(new UserStatsDto
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    AdminUsers = adminUsers,
                    CustomerUsers = customerUsers,
                    NewUsersThisMonth = newUsersThisMonth
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
    
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalQuotes { get; set; }
        public int PendingQuotes { get; set; }
        public int TotalSales { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<RecentQuoteDto> RecentQuotes { get; set; } = new();
        public List<MonthlyStatDto> MonthlyQuoteStats { get; set; } = new();
        public List<ProductStatDto> TopProducts { get; set; } = new(); // NUEVO
    }

    public class RecentQuoteDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; } // NUEVO
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
    }

    public class MonthlyStatDto
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Value { get; set; }
    }
    
    public class ProductStatDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuotesCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int CustomerUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
    }
}