namespace AGROPURE.Models.DTOs
{
    public class UpdateQuoteStatusDto
    {
        [Required]
        public QuoteStatus Status { get; set; }

        public string? AdminNotes { get; set; }
    }
}
