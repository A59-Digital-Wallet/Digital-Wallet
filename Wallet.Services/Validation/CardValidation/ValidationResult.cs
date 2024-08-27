using Wallet.Data.Models.Enums;

namespace Wallet.Services.Validation.CardValidation
{
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = new List<string>();
        public CardNetwork CardNetwork { get; set; }
    }

}
