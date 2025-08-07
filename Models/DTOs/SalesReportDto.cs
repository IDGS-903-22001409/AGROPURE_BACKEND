namespace AGROPURE.Models.DTOs
{
    public class SalesReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalSales { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<MonthlySalesDto> MonthlySales { get; set; } = new();
        public List<ProductSalesDto> TopProducts { get; set; } = new();
        public List<CustomerSalesDto> TopCustomers { get; set; } = new();
    }

    public class MonthlySalesDto
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
    }

    public class ProductSalesDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CustomerSalesDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
