using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests.MoneyRequestServiceTests
{
    [TestClass]
    public class MoneyRequestServiceTests
    {
        private Mock<IMoneyRequestRepository> _mockMoneyRequestRepository;
        private Mock<IUserService> _mockUserService;
        private Mock<IWalletRepository> _mockWalletRepository;
        private Mock<ITransactionService> _mockTransactionService;
        private Mock<ICurrencyExchangeService> _mockCurrencyExchangeService;
        private MoneyRequestService _moneyRequestService;

        [TestInitialize]
        public void Setup()
        {
            _mockMoneyRequestRepository = new Mock<IMoneyRequestRepository>();
            _mockUserService = new Mock<IUserService>();
            _mockWalletRepository = new Mock<IWalletRepository>();
            _mockTransactionService = new Mock<ITransactionService>();
            _mockCurrencyExchangeService = new Mock<ICurrencyExchangeService>();

            _moneyRequestService = new MoneyRequestService(
                _mockMoneyRequestRepository.Object,
                _mockUserService.Object,
                _mockWalletRepository.Object,
                _mockTransactionService.Object,
                _mockCurrencyExchangeService.Object
            );
        }

        [TestMethod]
        public async Task CreateMoneyRequestAsync_Should_Create_Request_And_Return_Response()
        {
            // Arrange
            var requestDto = new MoneyRequestCreateDTO
            {
                RecipientId = "recipient1",
                Amount = 100m,
                Description = "Test Request",
                RequestedCurrency = "USD"
            };
            var requesterId = "requester1";
            var recipient = new AppUser { Id = requestDto.RecipientId };

            _mockUserService.Setup(us => us.GetUserByIdAsync(requestDto.RecipientId))
                .ReturnsAsync(recipient);

            _mockMoneyRequestRepository.Setup(mr => mr.CreateAsync(It.IsAny<MoneyRequest>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _moneyRequestService.CreateMoneyRequestAsync(requestDto, requesterId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(requestDto.Amount, result.Amount);
            Assert.AreEqual(requestDto.Description, result.Description);
            _mockMoneyRequestRepository.Verify(mr => mr.CreateAsync(It.IsAny<MoneyRequest>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Recipient not found.")]
        public async Task CreateMoneyRequestAsync_Should_Throw_If_Recipient_Not_Found()
        {
            // Arrange
            var requestDto = new MoneyRequestCreateDTO { RecipientId = "recipient1", RequestedCurrency = "USD" };
            var requesterId = "requester1";

            _mockUserService.Setup(us => us.GetUserByIdAsync(requestDto.RecipientId))
                .ReturnsAsync((AppUser)null);

            // Act
            await _moneyRequestService.CreateMoneyRequestAsync(requestDto, requesterId);
        }


        [TestMethod]
        public async Task GetReceivedRequestsAsync_Should_Return_Requests()
        {
            // Arrange
            var recipientId = "recipient1";
            var requests = new List<MoneyRequest>
    {
        new MoneyRequest
        {
            Id = 1,
            RequesterId = "requester1",
            RecipientId = recipientId,
            Amount = 50m,
            Requester = new AppUser { UserName = "Requester1" } // Mock the Requester
        },
        new MoneyRequest
        {
            Id = 2,
            RequesterId = "requester2",
            RecipientId = recipientId,
            Amount = 75m,
            Requester = new AppUser { UserName = "Requester2" } // Mock the Requester
        }
    };

            _mockMoneyRequestRepository.Setup(mr => mr.GetReceivedRequestsAsync(recipientId))
                .ReturnsAsync(requests);

            // Act
            var result = await _moneyRequestService.GetReceivedRequestsAsync(recipientId);

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(50m, result.First().Amount);
            Assert.AreEqual("Requester1", result.First().UserName); // Assert the Requester.UserName
            _mockMoneyRequestRepository.Verify(mr => mr.GetReceivedRequestsAsync(recipientId), Times.Once);
        }
        [TestMethod]
        public async Task GetMoneyRequestByIdAsync_Should_Return_Request()
        {
            // Arrange
            var requestId = 1;
            var request = new MoneyRequest
            {
                Id = requestId,
                RequesterId = "requester1",
                Amount = 50m,
                Requester = new AppUser { UserName = "Requester1" } // Mock the Requester
            };

            _mockMoneyRequestRepository.Setup(mr => mr.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            // Act
            var result = await _moneyRequestService.GetMoneyRequestByIdAsync(requestId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(50m, result.Amount);
            Assert.AreEqual("Requester1", result.UserName); // Assert the Requester.UserName
            _mockMoneyRequestRepository.Verify(mr => mr.GetByIdAsync(requestId), Times.Once);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Money request not found.")]
        public async Task GetMoneyRequestByIdAsync_Should_Throw_If_Request_Not_Found()
        {
            // Arrange
            var requestId = 1;

            _mockMoneyRequestRepository.Setup(mr => mr.GetByIdAsync(requestId))
                .ReturnsAsync((MoneyRequest)null);

            // Act
            await _moneyRequestService.GetMoneyRequestByIdAsync(requestId);
        }
        [TestMethod]
        public async Task UpdateMoneyRequestStatusAsync_Should_Update_Request_Status()
        {
            // Arrange
            var requestId = 1;
            var request = new MoneyRequest { Id = requestId, Status = RequestStatus.Pending };

            _mockMoneyRequestRepository.Setup(mr => mr.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            _mockMoneyRequestRepository.Setup(mr => mr.UpdateAsync(request))
                .Returns(Task.CompletedTask);

            // Act
            await _moneyRequestService.UpdateMoneyRequestStatusAsync(requestId, RequestStatus.Approved);

            // Assert
            Assert.AreEqual(RequestStatus.Approved, request.Status);
            _mockMoneyRequestRepository.Verify(mr => mr.UpdateAsync(request), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Money request not found.")]
        public async Task UpdateMoneyRequestStatusAsync_Should_Throw_If_Request_Not_Found()
        {
            // Arrange
            var requestId = 1;

            _mockMoneyRequestRepository.Setup(mr => mr.GetByIdAsync(requestId))
                .ReturnsAsync((MoneyRequest)null);

            // Act
            await _moneyRequestService.UpdateMoneyRequestStatusAsync(requestId, RequestStatus.Approved);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ApproveMoneyRequestAsync_Should_Throw_When_Request_Not_Found()
        {
            // Arrange
            var requestId = 1;
            var senderId = "sender1";

            _mockMoneyRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync((MoneyRequest)null);

            // Act
            await _moneyRequestService.ApproveMoneyRequestAsync(requestId, senderId);
        }

        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public async Task ApproveMoneyRequestAsync_Should_Throw_When_Sender_Not_Recipient()
        {
            // Arrange
            var requestId = 1;
            var senderId = "sender1";
            var request = new MoneyRequest { Id = requestId, RecipientId = "otherRecipient" };

            _mockMoneyRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            // Act
            await _moneyRequestService.ApproveMoneyRequestAsync(requestId, senderId);
        }

        [TestMethod]
        public async Task ApproveMoneyRequestAsync_Should_Create_Transaction_When_Valid()
        {
            // Arrange
            var requestId = 1;
            var senderId = "recipient1";
            var request = new MoneyRequest
            {
                Id = requestId,
                RecipientId = senderId,
                Amount = 100,
                RequestedCurrency = Currency.USD,
                Status = RequestStatus.Pending
            };

            _mockMoneyRequestRepository.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            _mockMoneyRequestRepository.Setup(repo => repo.UpdateAsync(It.IsAny<MoneyRequest>()))
                .Returns(Task.CompletedTask);

            var senderWallet = new UserWallet
            {
                Id = 1,
                Currency = Currency.USD,
                Balance = 1000
            };

            var recipientWallet = new UserWallet
            {
                Id = 2,
                Currency = Currency.USD,
                Balance = 100
            };

            _mockWalletRepository.Setup(repo => repo.GetUserWalletsAsync(senderId))
                .ReturnsAsync(new List<UserWallet> { senderWallet });

            _mockWalletRepository.Setup(repo => repo.GetUserWalletsAsync(request.RequesterId))
                .ReturnsAsync(new List<UserWallet> { recipientWallet });

            _mockTransactionService.Setup(ts => ts.CreateTransactionAsync(It.IsAny<TransactionRequestModel>(), It.IsAny<string>(), null))
                .Returns(Task.CompletedTask);

            // Act
            await _moneyRequestService.ApproveMoneyRequestAsync(requestId, senderId);

            // Assert
            _mockMoneyRequestRepository.Verify(repo => repo.UpdateAsync(It.IsAny<MoneyRequest>()), Times.Once);
            _mockTransactionService.Verify(ts => ts.CreateTransactionAsync(It.IsAny<TransactionRequestModel>(), senderId, null), Times.Once);
        }


    }

}
