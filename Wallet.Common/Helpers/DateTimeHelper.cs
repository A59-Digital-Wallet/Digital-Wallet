using System.Globalization;

namespace Wallet.Common.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime ConvertToDateTime(string expiryDate)
        {
            if (DateTime.TryParseExact(
                expiryDate,
                "MM/yy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDate))
            {
                return parsedDate;
            }
            else
            {
                throw new ArgumentException("Invalid expiry date format.");
            }
        }
    }
}
