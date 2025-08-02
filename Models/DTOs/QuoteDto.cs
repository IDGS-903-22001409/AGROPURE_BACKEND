using AGROPURE.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Models.DTOs
{
    public class QuoteDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public QuoteStatus Status { get; set; }
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string StatusText => Status.ToString();
    }

    public class CreateQuoteDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdateQuoteStatusDto
    {
        [Required]
        public QuoteStatus Status { get; set; }

        public string? AdminNotes { get; set; }
    }
}