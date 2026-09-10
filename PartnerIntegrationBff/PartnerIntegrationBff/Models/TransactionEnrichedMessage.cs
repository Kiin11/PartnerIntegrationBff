namespace PartnerIntegrationBff.Models
{
    public class TransactionEnrichedMessage
    {
        public string PartnerId { get; set; }
        public string TransactionReference { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime IngestedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
