using Wallet.Common.Helpers;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;

namespace Wallet.Services.Validation.CardValidation
{
    public class CardValidation
    {
        public ValidationResult Validate(CardRequest cardRequest)
        {
            DateTime expiryDate = DateTimeHelper.ConvertToDateTime(cardRequest.ExpiryDate);

            ValidationResult result = new ValidationResult();

            // Validate Card Number using Luhn's Algorithm
            if (!CardNumberValidation(cardRequest.CardNumber))
            {
                result.Errors.Add("Invalid card number.");
            }

            // Determine and validate the Card Network
            var cardNetwork = GetCardNetwork(cardRequest.CardNumber);
            if (cardNetwork == CardNetwork.Unknown)
            {
                result.Errors.Add("Unknown card network.");
            }

            // Validate Card Length based on the Card Network
            if (!ValidateCardLength(cardRequest.CardNumber, cardNetwork))
            {
                result.Errors.Add("Invalid card number length.");
            }

            // Validate Expiration Date
            if (!ValidateExpirationDate(expiryDate))
            {
                result.Errors.Add("The card is nearing its expiration. Please provide a card with more than one month of validity remaining.");
            }

            // Validate CVV
            if (!ValidateCVV(cardRequest.CVV, cardNetwork))
            {
                result.Errors.Add("Invalid CVV.");
            }

            // Validate Cardholder Name
            if (!ValidateCardHolderName(cardRequest.CardHolderName))
            {
                result.Errors.Add("Invalid cardholder name.");
            }

            // If all validations pass, set the card network
            if (result.IsValid)
            {
                result.CardNetwork = cardNetwork;
            }

            return result;
        }

        public bool ValidateExpirationDate(DateTime expiryDate)
        {
            DateTime now = DateTime.UtcNow;

            TimeSpan difference = expiryDate - now;

            if (difference.TotalDays <= 30)
            {
                return false;
            }
            return true;
        }

        private bool CardNumberValidation(string cardNumber)
        {
            //Luhn's Algorithm
            // Remove any spaces or dashes from the card number
            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            int sum = 0;
            bool alternate = false;

            // Loop through the card number digits starting from the rightmost digit
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                // Convert the current character to an integer
                int digit = int.Parse(cardNumber[i].ToString());

                // If alternate is true, multiply the digit by 2
                if (alternate)
                {
                    digit *= 2;

                    // If the result is greater than 9, subtract 9
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                // Add the digit to the sum
                sum += digit;

                // Flip the alternate boolean
                alternate = !alternate;
            }

            // If the sum is divisible by 10, the card number is valid
            return sum % 10 == 0;
        }

        private bool ValidateCardLength(string cardNumber, CardNetwork cardNetwork)
        {
            switch (cardNetwork)
            {
                case CardNetwork.Visa:
                    return cardNumber.Length == 16 || cardNumber.Length == 13;
                case CardNetwork.MasterCard:
                    return cardNumber.Length == 16;
                case CardNetwork.AmericanExpress:
                    return cardNumber.Length == 15;
                case CardNetwork.Discover:
                    return cardNumber.Length == 16;
                default:
                    return false;
            }
        }

        private CardNetwork GetCardNetwork(string cardNumber)
        {
            string iin = cardNumber.Substring(0, 6);

            if (iin.StartsWith("4"))
            {
                return CardNetwork.Visa;
            }
            if (iin.StartsWith("51") || iin.StartsWith("52") || iin.StartsWith("53") || iin.StartsWith("54") || iin.StartsWith("55"))
            {
                return CardNetwork.MasterCard;
            }
            if (iin.StartsWith("34") || iin.StartsWith("37"))
            {
                return CardNetwork.AmericanExpress;
            }
            if (iin.StartsWith("6011") || iin.StartsWith("65"))
            {
                return CardNetwork.Discover;
            }

            return CardNetwork.Unknown;
        }

        private bool ValidateCVV(string cvv, CardNetwork cardNetwork)
        {
            if (string.IsNullOrWhiteSpace(cvv))
            {
                return false;
            }

            // Ensure CVV is numeric
            if (!cvv.All(char.IsDigit))
            {
                return false;
            }

            switch (cardNetwork)
            {
                case CardNetwork.AmericanExpress:
                    return cvv.Length == 4;
                default:
                    return cvv.Length == 3;
            }
        }

        private bool ValidateCardHolderName(string cardHolderName)
        {
            if (cardHolderName.Any(char.IsDigit))
            {
                return false;
            }
            return !string.IsNullOrWhiteSpace(cardHolderName);
        }
    }
}
