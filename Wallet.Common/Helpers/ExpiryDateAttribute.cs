using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Wallet.Common.Helpers
{
    public class ExpiryDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is string expiryDate)
            {
                return DateTime.TryParseExact(
                    expiryDate,
                    "MM/yy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _);
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be in MM/yy format.";
        }
    }
}
