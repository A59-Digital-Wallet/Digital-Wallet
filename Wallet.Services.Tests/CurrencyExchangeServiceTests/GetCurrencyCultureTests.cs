using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests.CurrencyExchangeServiceTests
{
    [TestClass]
    public class GetCurrencyCultureTests
    {
        [TestMethod]
        public void ReturnCorrectCulture_ForUSD()
        {
            // Arrange
            var currencyCode = "USD";
            var expectedCulture = "en-US";

            // Act
            var result = CurrencyHelper.GetCurrencyCulture(currencyCode);

            // Assert
            Assert.AreEqual(expectedCulture, result);
        }

        [TestMethod]
        public void ReturnCorrectCulture_ForEUR()
        {
            // Arrange
            var currencyCode = "EUR";
            var expectedCulture = "fr-FR";

            // Act
            var result = CurrencyHelper.GetCurrencyCulture(currencyCode);

            // Assert
            Assert.AreEqual(expectedCulture, result);
        }

        [TestMethod]
        public void ReturnCorrectCulture_ForBGN()
        {
            // Arrange
            var currencyCode = "BGN";
            var expectedCulture = "bg-BG";

            // Act
            var result = CurrencyHelper.GetCurrencyCulture(currencyCode);

            // Assert
            Assert.AreEqual(expectedCulture, result);
        }

        [TestMethod]
        public void ReturnCorrectCulture_ForGBP()
        {
            // Arrange
            var currencyCode = "GBP";
            var expectedCulture = "en-GB";

            // Act
            var result = CurrencyHelper.GetCurrencyCulture(currencyCode);

            // Assert
            Assert.AreEqual(expectedCulture, result);
        }

        [TestMethod]
        public void ReturnDefaultCulture_ForUnknownCurrency()
        {
            // Arrange
            var currencyCode = "XYZ";
            var expectedCulture = "en-US"; // Default culture

            // Act
            var result = CurrencyHelper.GetCurrencyCulture(currencyCode);

            // Assert
            Assert.AreEqual(expectedCulture, result);
        }
    }
}
