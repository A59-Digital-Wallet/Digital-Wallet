using Wallet.Data.Models.Enums;

namespace Wallet.DTO.Response
{
    public class MoneyRequestResponseDTO
    {
        public int Id { get; set; }
        public string RequesterId { get; set; }
        public string RecipientId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string RequestedCurrency { get; set; }
    }
}
