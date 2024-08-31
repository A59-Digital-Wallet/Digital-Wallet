using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Validation.TransactionValidation;

namespace Wallet.Services.Tests
{
    public class MockTransactionValidator
    {
        public Mock<ITransactionValidator> GetMockTransactionValidator()
        {
            var mockValidator = new Mock<ITransactionValidator>();

            // Mocking the ValidateOverdraftAndBalance method
            mockValidator.Setup(v => v.ValidateOverdraftAndBalance(It.IsAny<UserWallet>(), It.IsAny<decimal>()))
                         .Verifiable();

            // Mocking the ValidateWalletOwnership method
            mockValidator.Setup(v => v.ValidateWalletOwnership(It.IsAny<UserWallet>(), It.IsAny<string>()))
                         .Verifiable();

            // Mocking the IsHighValueTransaction method
            mockValidator.Setup(v => v.IsHighValueTransaction(It.IsAny<TransactionRequestModel>(), It.IsAny<UserWallet>()))
                         .Returns(false);

            return mockValidator;
        }
    }
}
